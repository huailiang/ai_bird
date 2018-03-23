import atexit
import io
import glob
import json
import logging
import numpy as np
import os
import socket
import subprocess
import struct

from dqn.DQN_Brain import DeepQNetwork
from exception import UnityEnvironmentException, UnityActionException, UnityTimeOutException
from sys import platform

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("unity")


class UnityEnvironment(object):
    def __init__(self, file_name, base_port=5006):
        atexit.register(self.close)
        self.port = base_port 
        self._buffer_size = 10240
        self._loaded = False
        self._open_socket = False
        logger.info("unity env try created, socket with port:{}".format(str(self.port)))

        try:
            # Establish communication socket
            self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self._socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self._socket.bind(("localhost", self.port))
            self._open_socket = True
        except socket.error:
            self._open_socket = True
            self.close()
            raise socket.error("Couldn't launch new environment "
                               "You may need to manually close a previously opened environment "
                               "or use a different worker number.")

        cwd = os.getcwd()
        file_name = (file_name.strip()
                     .replace('.app', '').replace('.exe', '').replace('.x86_64', '').replace('.x86', ''))
        true_filename = os.path.basename(os.path.normpath(file_name))
        launch_string = None
        if platform == "linux" or platform == "linux2":
            candidates = glob.glob(os.path.join(cwd, file_name) + '.x86_64')
            if len(candidates) == 0:
                candidates = glob.glob(os.path.join(cwd, file_name) + '.x86')
            if len(candidates) == 0:
                candidates = glob.glob(file_name + '.x86_64')
            if len(candidates) == 0:
                candidates = glob.glob(file_name + '.x86')
            if len(candidates) > 0:
                launch_string = candidates[0]

        elif platform == 'darwin':
            candidates = glob.glob(os.path.join(cwd, file_name + '.app', 'Contents', 'MacOS', true_filename))
            if len(candidates) == 0:
                candidates = glob.glob(os.path.join(file_name + '.app', 'Contents', 'MacOS', true_filename))
            if len(candidates) == 0:
                candidates = glob.glob(os.path.join(cwd, file_name + '.app', 'Contents', 'MacOS', '*'))
            if len(candidates) == 0:
                candidates = glob.glob(os.path.join(file_name + '.app', 'Contents', 'MacOS', '*'))
            if len(candidates) > 0:
                launch_string = candidates[0]
        elif platform == 'win32':
            candidates = glob.glob(os.path.join(cwd, file_name + '.exe'))
            if len(candidates) == 0:
                candidates = glob.glob(file_name + '.exe')
            if len(candidates) > 0:
                launch_string = candidates[0]
        if launch_string is None:
            self.close()
            raise UnityEnvironmentException("Couldn't launch the {0} environment. "
                                            "Provided filename does not match any environments."
                                            .format(true_filename))
        else:
            # Launch Unity environment
            proc1 = subprocess.Popen([launch_string,'--port', str(self.port)])

        self._socket.settimeout(60)
        try:
            try:
                self._socket.listen(1)
                self._conn, _ = self._socket.accept()
                self._conn.settimeout(30)
                p = self._conn.recv(self._buffer_size).decode('utf-8')
                p = json.loads(p)
                # print p
            except socket.timeout as e:
                raise UnityTimeOutException(
                    "The Unity environment took too long to respond. Make sure {} does not need user interaction to "
                    "launch and that the Academy and the external Brain(s) are attached to objects in the Scene."
                    .format(str(file_name)))

            self._data = {}
            self._global_done = None
            self._log_path = p["logPath"]
            self._alpha = p["alpha"]
            self._epsilon = p["epsilon"]
            self._gamma = p["gamma"]
            self._states = p["states"]
            self._actions = p["actions"]
            self.n_action = len(self._actions)
            logger.info("action len:{}".format(str(self.n_action)))
            self.step = 1
            # self._brain = QLearningTable(self._actions,self._states, self._alpha,self._gamma,self._epsilon)
            self.RL = DeepQNetwork(n_actions = self.n_action,
                n_features = 1, 
                learning_rate = self._alpha,
                reward_decay = self._gamma,
                e_greedy = self._epsilon,
                replace_target_iter = 200,
                memory_size = 2000,
                output_graph = True)
            self._loaded = True
            self._recv_bytes()
            logger.info("started successfully!")
        except UnityEnvironmentException:
            proc1.kill()
            self.close()
            raise


    def __str__(self):
        return "unity env args, socket port:{0}, epsilon:{1}, gamma:{2}".format(str(self.port),str(self._log_path),str(self._gamma))

    def _recv_bytes(self):
        try:
            # if not self._loaded:
            #     return
            s = self._conn.recv(self._buffer_size)
            message_length = struct.unpack("I", bytearray(s[:4]))[0]
            s = s[4:]
            while len(s) != message_length:
                s += self._conn.recv(self._buffer_size)
            p = json.loads(s)
            code = p["Code"]
            # logging.info("rcv code:"+str(code))
            if code == "EEXIT":
                self.close()
            elif code == "CHOIC":
                state = p["state"]
                self._send_choice(state)
                self._recv_bytes()
            elif code == "UPDAT":
                self._to_learn(p)
                self._recv_bytes()
            else:
                logging.error("\nunknown code:{0}".format(str(code)))
                self._recv_bytes()
        except socket.timeout as e:
            logger.warning("timeout, will close socket")
            self.close()

    def TransBrainState(self,state):
        state = np.array(state)
        return state[np.newaxis,]


    def _send_choice(self, state):
        try:
            
            # state = np.array(state)
            # state = state[np.newaxis,]
            state = self.TransBrainState(state)
            # logging.info("get state:{}".format(str(state)))
            action=self.RL.choose_action(state)
            
            if action == 0:
                action ="pad"
            else:
                action ="stay"
            # logger.info("choose_action is:{0}".format(str(action)))
            self._conn.send(action)
            # logger.info("send action finish")
        except UnityEnvironmentException:
            logger.warning("send choice err")
            raise 

    def _to_learn(self,j):
        state_ = j["state_"]
        state  = j["state"]
        action = j["action"]
        rewd = j["rewd"]
        # logger.info("state:{0} action:{1} rewd:{2}".format(str(state),str(action),str(rewd),str(type(action))))

        if action == True:
            action = 0 #"pad"
        else:
            action = 1 # "stay"
        
        state=self.TransBrainState(state)
        state_=self.TransBrainState(state_)
        # logger.info("to_store:{0}".format(str(action)))
        self.RL.store_transition(state,action,rewd,state_)
        # logger.info("state:{0} action:{1} rewd:{2}".format(str(state),str(action),str(rewd)))
        if self.step % 5 == 0 :
            self.RL.learn()
        self.step=self.step+1
        


    def close(self):
        logger.info("env closed")
        if self._loaded & self._open_socket:
            self._conn.send(b"EXIT")
            self._conn.close()
        if self._open_socket:
            self._socket.close()
            self._loaded = False
        else:
            raise UnityEnvironmentException("No Unity environment is loaded.")
