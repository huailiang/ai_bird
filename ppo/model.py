# coding=utf8


"""
load models trained 
choice action and send to client
"""

import logging
import numpy as np
import tensorflow as tf
from tensorflow.python.framework import graph_util

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("bird")


class Model(object):

    def __init__(self):
        self.model_path = './models/ppo/'
        self.sess = tf.Session()
        self.xstate = tf.placeholder(tf.float32, [None, 1], name = 'state')

        with open(self.model_path+"ppo.bytes",'rb') as f:
            self.graph_def = tf.GraphDef()
            self.graph_def.ParseFromString(f.read())
            self.output = tf.import_graph_def(self.graph_def,
                input_map={'state:0': self.xstate},
                return_elements=['pi/probweights:0'])

    def update(self, s, a, r):
        self.sess.run(self.update_oldpi_op)
        adv = self.sess.run(self.advantage, {self.tfs: s, self.tfdc_r: r})
        #update actor
        [self.sess.run(self.atrain_op, {self.tfs: s, self.tfa: a, self.tfadv: adv}) for _ in range(A_UPDATE_STEPS)]
        # update critic
        [self.sess.run(self.ctrain_op, {self.tfs: s, self.tfdc_r: r}) for _ in range(C_UPDATE_STEPS)]


    def choose_action(self, s):
        
        result = self.sess.run(self.output, feed_dict = { self.xstate: [s]})
        prob_weights=result[0]
        action = np.random.choice(range(prob_weights.shape[1]), p=prob_weights.ravel())
        return action
