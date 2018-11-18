# coding=utf8

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
import sys

from brain import PPO
from exception import UnityEnvironmentException, UnityActionException, UnityTimeOutException
from sys import platform

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("unity")

# 设置最大递归次数
sys.setrecursionlimit(1000000) 

GAMMA = 0.9
BATCH = 8
EP_LEN = 200
all_ep_r = []

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

            if not os.path.exists("models"):
                os.makedirs("models")

            self.ppo = PPO()
            self.all_ep_r = []
            self.buffer_s, self.buffer_a, self.buffer_r = [], [], []
            self.ep_r = 0
            self.tick = 0
            self._loaded = True
            self._recv_bytes()
            logger.info("server quit!")
        except UnityEnvironmentException:
            proc1.kill()
            self.close()
            raise


    def __str__(self):
        return "unity env args, socket port:{0}".format(str(self.port))

    def _recv_bytes(self):
        try:
            s = self._conn.recv(self._buffer_size)
            message_length = struct.unpack("I", bytearray(s[:4]))[0]
            s = s[4:]
            while len(s) != message_length:
                s += self._conn.recv(self._buffer_size)
            p = json.loads(s)
            code = p["Code"]
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


    def _send_choice(self, state):
        try:
            obvs =  np.array([state])
            # logger.info("recv state:{0}".format(str(state)))
            action = self.ppo.choose_action(obvs)
            if action == 1:
                action="pad"
            else:
                action="stay"
            logger.info("send action:{0}",format(str(action)))
            self._conn.send(action)
            # logger.info("send action2:{0}",format(str(action)))
        except UnityEnvironmentException:
            raise 

    def _to_learn(self,j):
        state_ = j["state_"]
        state  = j["state"]
        action = j["action"]
        rewd = j["rewd"]
        if action:
            action =1  #"pad"
        else:
            action =0 #"stay"
        # logger.info("get action is:{0}, state:{1}, rewd:{2}".format(str(action),str(state), str(rewd)))
        nps=np.array([state])[np.newaxis, :]
        nps_=np.array([state_])[np.newaxis, :]
        npa=np.array([action])
        npr = np.array([rewd])[np.newaxis, :]
        self.buffer_s.append(state)
        self.buffer_a.append(action)
        self.buffer_r.append((rewd+8)/8)
        self.ep_r += rewd
        self.tick += 1
        if self.tick % BATCH == 0 :
            
            v_s_ = self.ppo.get_v(nps_)
            discounted_r = []
            for r in self.buffer_r[::-1]:
                v_s_ = r+GAMMA*v_s_
                discounted_r.append(v_s_)
            discounted_r.reverse()

            bs, ba, br= np.vstack(self.buffer_s), self.buffer_a, np.array(discounted_r)[:, np.newaxis]
            self.buffer_s, self.buffer_a, self.buffer_r=[],[],[]
            self.ppo.update(bs, ba, br)

    def close(self):
        logger.info("env closed")
        if self._loaded & self._open_socket:
            self._conn.send(b"EXIT")
            self._conn.close()
            self.ppo.exporrt_graph()
        if self._open_socket:
            self._socket.close()
            self._loaded = False
        else:
            raise UnityEnvironmentException("No Unity environment is loaded.")
