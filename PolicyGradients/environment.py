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

from brain import PolicyGradient
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
            self._brain = PolicyGradient(len(self._actions),1, self._alpha,self._gamma,True)
            self._loaded = True
            self._recv_bytes()
            logger.info("server quit!")
        except UnityEnvironmentException:
            proc1.kill()
            self.close()
            raise


    def __str__(self):
        return "unity env args, socket port:{0}, epsilon:{1}, gamma:{2}".format(str(self.port),str(self._log_path),str(self._gamma))

    def _recv_bytes(self):
        try:
            s = self._conn.recv(self._buffer_size)
            message_length = struct.unpack("I", bytearray(s[:4]))[0]
            s = s[4:]
            while len(s) != message_length:
                s += self._conn.recv(self._buffer_size)
            p = json.loads(s)
            code = p["Code"]
            # logging.info("rcv: "+s+" code:"+str(code))
            if code == "EEXIT":
                self.close()
            elif code == "CHOIC":
                state = p["state"]
                self._send_choice(state)
                self._recv_bytes()
            elif code == "UPDAT":
                self._to_store(p)
                self._recv_bytes()
            elif code == "EPSOL":
                self._to_learn(p)
                self._recv_bytes()
            else:
                logging.error("\nunknown code:{0}".format(str(code)))
                self._recv_bytes()
        except socket.timeout as e:
            logger.warning("timeout, will close socket")
            self.close()



    def _send_choice(self, state):
        try:
            # logging.info("recv state:{}".format(str(state)))
            obvs =  np.array([state])
            action = self._brain.choose_action(obvs)
            logger.info("get action is:{}".format(str(action)))
            if action == 1:
                action="pad"
            else:
                action="stay"
            self._conn.send(action)
        except UnityEnvironmentException:
            raise 

    def _to_store(self,j):
        state_ = j["state_"]
        state  = j["state"]
        action = j["action"]
        rewd = j["rewd"]
        if action:
            action =1  #"pad"
        else:
            action =0 #"stay"
        # logger.info("state:{0} action:{1}".format(str(state),str(action)))
        self._brain.store_transition(state,action,rewd)

    def _to_learn(self,j):
        self._brain.learn()
        

    def close(self):
        logger.info("env closed")
        if not self._brain:
            self._brain.export()
        if self._loaded & self._open_socket:
            self._conn.send(b"EXIT")
            self._conn.close()
        if self._open_socket:
            self._socket.close()
            self._loaded = False
        else:
            raise UnityEnvironmentException("No Unity environment is loaded.")
