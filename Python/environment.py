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

from brain import QLearningTable
from exception import UnityEnvironmentException, UnityActionException, UnityTimeOutException

from sys import platform

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("unity")


class UnityEnvironment(object):
    def __init__(self, file_name, worker_id=0,base_port=5005):
        atexit.register(self.close)
        self.port = base_port + worker_id
        self._buffer_size = 12000
        self._loaded = False
        self._open_socket = False

        try:
            # Establish communication socket
            self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self._socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self._socket.bind(("localhost", self.port))
            self._open_socket = True
        except socket.error:
            self._open_socket = True
            self.close()
            raise socket.error("Couldn't launch new environment because worker number {} is still in use. "
                               "You may need to manually close a previously opened environment "
                               "or use a different worker number.".format(str(worker_id)))

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

        self._socket.settimeout(120)
        try:
            try:
                self._socket.listen(1)
                self._conn, _ = self._socket.accept()
                self._conn.settimeout(30)
                p = self._conn.recv(self._buffer_size).decode('utf-8')
                p = json.loads(p)
            except socket.timeout as e:
                raise UnityTimeOutException(
                    "The Unity environment took too long to respond. Make sure {} does not need user interaction to "
                    "launch and that the Academy and the external Brain(s) are attached to objects in the Scene."
                    .format(str(file_name)))

            self._data = {}
            self._global_done = None
            self._log_path = p["logPath"]
            self._brains = {}
            self._brain_names = p["brainNames"]
            self._loaded = True
            logger.info("\n  started successfully!")
         except UnityEnvironmentException:
            proc1.kill()
            self.close()
            raise

    def _recv_bytes(self):
        try:
            s = self._conn.recv(self._buffer_size)
            message_length = struct.unpack("I", bytearray(s[:4]))[0]
            s = s[4:]
            while len(s) != message_length:
                s += self._conn.recv(self._buffer_size)
        except socket.timeout as e:
            raise UnityTimeOutException("The environment took too long to respond.", self._log_path)
        return s

    def _get_state_dict(self):
        state = self._recv_bytes().decode('utf-8')
        self._conn.send(b"RECEIVED")
        state_dict = json.loads(state)
        return state_dict

    def reset(self, train_mode=True, config=None, progress=None):
        old_lesson = self._curriculum.get_lesson_number()
        if config is None:
            config = self._curriculum.get_lesson(progress)
            if old_lesson != self._curriculum.get_lesson_number():
                logger.info("\nLesson changed. Now in Lesson {0} : \t{1}"
                            .format(self._curriculum.get_lesson_number(),
                                    ', '.join([str(x) + ' -> ' + str(config[x]) for x in config])))
        elif config != {}:
            logger.info("\nAcademy Reset with parameters : \t{0}"
                        .format(', '.join([str(x) + ' -> ' + str(config[x]) for x in config])))
        for k in config:
            if (k in self._resetParameters) and (isinstance(config[k], (int, float))):
                self._resetParameters[k] = config[k]
            elif not isinstance(config[k], (int, float)):
                raise UnityEnvironmentException(
                    "The value for parameter '{0}'' must be an Integer or a Float.".format(k))
            else:
                raise UnityEnvironmentException("The parameter '{0}' is not a valid parameter.".format(k))

        if self._loaded:
            self._conn.send(b"RESET")
            try:
                self._conn.recv(self._buffer_size)
            except socket.timeout as e:
                raise UnityTimeOutException("The environment took too long to respond.", self._log_path)
            self._conn.send(json.dumps({"train_model": train_mode, "parameters": config}).encode('utf-8'))
            return self._get_state()
        else:
            raise UnityEnvironmentException("No Unity environment is loaded.")

    def _get_state(self):
        self._data = {}
        for index in range(self._num_brains):
            state_dict = self._get_state_dict()
            b = state_dict["brain_name"]
            n_agent = len(state_dict["agents"])
            try:
                if self._brains[b].state_space_type == "continuous":
                    states = np.array(state_dict["states"]).reshape((n_agent, self._brains[b].state_space_size))
                else:
                    states = np.array(state_dict["states"]).reshape((n_agent, 1))
            except UnityActionException:
                raise UnityActionException("Brain {0} has an invalid state. "
                                           "Expecting {1} {2} state but received {3}."
                                           .format(b, n_agent if self._brains[b].state_space_type == "discrete"
                else str(self._brains[b].state_space_size * n_agent),
                                                   self._brains[b].state_space_type,
                                                   len(state_dict["states"])))
            memories = np.array(state_dict["memories"]).reshape((n_agent, self._brains[b].memory_space_size))
            rewards = state_dict["rewards"]
            dones = state_dict["dones"]
            # actions = state_dict["actions"]
            if n_agent > 0:
                actions = np.array(state_dict["actions"]).reshape((n_agent, -1))
            else:
                actions = np.array([])

            observations = []
            for o in range(self._brains[b].number_observations):
                obs_n = []
                for a in range(n_agent):
                    obs_n.append(self._get_state_image(self._brains[b].camera_resolutions[o]['blackAndWhite']))

                observations.append(np.array(obs_n))

            self._data[b] = BrainInfo(observations, states, memories, rewards, agents, dones, actions)

        try:
            self._global_done = self._conn.recv(self._buffer_size).decode('utf-8') == 'True'
        except socket.timeout as e:
            raise UnityTimeOutException("The environment took too long to respond.", self._log_path)

        return self._data

    def _send_action(self, action, memory, value):
        try:
            self._conn.recv(self._buffer_size)
        except socket.timeout as e:
            raise UnityTimeOutException("The environment took too long to respond.", self._log_path)
        action_message = {"action": action, "memory": memory, "value": value}
        self._conn.send(json.dumps(action_message).encode('utf-8'))


    def step(self, action=None, memory=None, value=None):
        action = {} if action is None else action
        value = {} if value is None else value
        if self._loaded and not self._global_done and self._global_done is not None:
            pass

        if not self._loaded:
            raise UnityEnvironmentException("No Unity environment is loaded.")
        elif self._global_done:
            raise UnityActionException("The episode is completed. Reset the environment with 'reset()'")
        elif self.global_done is None:
            raise UnityActionException(
                "You cannot conduct step without first calling reset. Reset the environment with 'reset()'")

    def close(self):
        if self._loaded & self._open_socket:
            self._conn.send(b"EXIT")
            self._conn.close()
        if self._open_socket:
            self._socket.close()
            self._loaded = False
        else:
            raise UnityEnvironmentException("No Unity environment is loaded.")
