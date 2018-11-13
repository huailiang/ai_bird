# coding=utf8

import logging
import numpy as np
import tensorflow as tf
from tensorflow.python.tools import freeze_graph

# reproducible
np.random.seed(1)
tf.set_random_seed(1)

import tensorflow as tf
import numpy as np

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("unity")

A_LR = 0.0001
C_LR = 0.0002
A_UPDATE_STEPS = 10
C_UPDATE_STEPS = 10
S_DIM, A_DIM = 1, 2
EPSILON = 0.2           



class PPO(object):

    def __init__(self):
        self.model_path = './models/ppo/'
        self.sess = tf.Session()
        self.tfs = tf.placeholder(tf.float32, [None, S_DIM], 'state')
        self.step = 1
         

        # critic
        with tf.variable_scope('critic'):
            l1 = tf.layers.dense(self.tfs, 100, tf.nn.relu)
            self.v = tf.layers.dense(l1, 1)
            self.tfdc_r = tf.placeholder(tf.float32, [None, 1], 'discounted_r')
            self.advantage = self.tfdc_r - self.v
            self.closs = tf.reduce_mean(tf.square(self.advantage))
            self.ctrain_op = tf.train.AdamOptimizer(C_LR).minimize(self.closs)

        # actor
        self.pi, pi_params = self._build_anet('pi', trainable=True)
        oldpi, oldpi_params = self._build_anet('oldpi', trainable=False)
        with tf.variable_scope('update_oldpi'):
            self.update_oldpi_op = [oldp.assign(p) for p, oldp in zip(pi_params, oldpi_params)]

        self.tfa = tf.placeholder(tf.int32, [None, ], 'action')
        self.tfadv = tf.placeholder(tf.float32, [None, 1], 'advantage')
        self.saver = tf.train.Saver() 
        a_indices = tf.stack([tf.range(tf.shape(self.tfa)[0], dtype=tf.int32), self.tfa], axis=1)
        pi_prob = tf.gather_nd(params=self.pi, indices=a_indices)   # shape=(None, )
        oldpi_prob = tf.gather_nd(params=oldpi, indices=a_indices)  # shape=(None, )
        ratio = pi_prob/oldpi_prob
        surr = ratio * self.tfadv                       # surrogate loss

        with tf.variable_scope('loss'):
            self.aloss = -tf.reduce_mean(tf.minimum(        # clipped surrogate objective
            surr,
            tf.clip_by_value(ratio, 1. - EPSILON, 1. + EPSILON) * self.tfadv))

        with tf.variable_scope('atrain'):
            self.atrain_op = tf.train.AdamOptimizer(A_LR).minimize(self.aloss)

        tf.summary.FileWriter("log/", self.sess.graph)
        self.sess.run(tf.global_variables_initializer())


    def update(self, s, a, r):
        self.sess.run(self.update_oldpi_op)
        adv = self.sess.run(self.advantage, {self.tfs: s, self.tfdc_r: r})

        #update actor
        [self.sess.run(self.atrain_op, {self.tfs: s, self.tfa: a, self.tfadv: adv}) for _ in range(A_UPDATE_STEPS)]

        # update critic
        [self.sess.run(self.ctrain_op, {self.tfs: s, self.tfdc_r: r}) for _ in range(C_UPDATE_STEPS)]

        self.step += 1
        self.saver.save(self.sess, self.model_path  + 'model.ckpt', global_step=self.step)
        tf.train.write_graph(self.sess.graph_def, self.model_path, 'raw_graph.pb',  as_text=False)
        logger.info('Saved Model')


    def _build_anet(self, name, trainable):
        with tf.variable_scope(name):
            l_a = tf.layers.dense(self.tfs, 200, tf.nn.relu, trainable=trainable)
            a_prob = tf.layers.dense(l_a, A_DIM, tf.nn.softmax, trainable=trainable)
        params = tf.get_collection(tf.GraphKeys.GLOBAL_VARIABLES, scope=name)
        return a_prob, params

    def choose_action(self, s):
        prob_weights = self.sess.run(self.pi, feed_dict={self.tfs: s[None, :]})
        action = np.random.choice(range(prob_weights.shape[1]),
                                      p=prob_weights.ravel())  # select action w.r.t the actions prob
        return action

    def get_v(self, s):
        return self.sess.run(self.v, {self.tfs: s})[0, 0]

    def process_graph(self):
        nodes = ["critic/discounted_r"]
        return nodes


    def exporrt_graph(self):
        target_nodes = ','.join(self.process_graph())
        ckpt = tf.train.get_checkpoint_state(self.model_path)
        print(ckpt)
        freeze_graph.freeze_graph(
            input_graph = self.model_path + 'raw_graph.pb',
            input_binary = True,
            input_checkpoint = ckpt.model_checkpoint_path,
            output_node_names = target_nodes,
            output_graph = (self.model_path + 'ppo.bytes'),
            clear_devices = True, 
            initializer_nodes = '', 
            input_saver = '', 
            restore_op_name = 'save/restore_all', 
            filename_tensor_name = 'save/Const:0')