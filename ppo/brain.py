# coding=utf8

import logging
import numpy as np
import tensorflow as tf
from tensorflow.python.framework import graph_util

# reproducible
np.random.seed(1)
tf.set_random_seed(1)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("bird")

A_LR = 0.001
C_LR = 0.001
A_UPDATE_STEPS = 10
C_UPDATE_STEPS = 10
S_DIM, A_DIM = 1, 2
EPSILON = 0.2           

class PPO(object):

    def __init__(self):
        self.model_path = './models/ppo/'
        self.sess = tf.Session()
        self.tfs = tf.placeholder(tf.float32, [None, S_DIM], name = 'state')

        # critic
        with tf.variable_scope('critic'):
            w_init = tf.random_normal_initializer(0., .1)
            lc = tf.layers.dense(self.tfs, 200, tf.nn.relu, kernel_initializer=w_init, name='lc')
            self.v = tf.layers.dense(lc, 1)
            self.tfdc_r = tf.placeholder(tf.float32, [None, 1], 'discounted_r')
            self.advantage = self.tfdc_r - self.v
            self.c_loss = tf.reduce_mean(tf.square(self.advantage))
            self.ctrain_op = tf.train.AdamOptimizer(C_LR).minimize(self.c_loss)

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
        # update advantage
        adv = self.sess.run(self.advantage, {self.tfs: s, self.tfdc_r: r})
        # update actor
        [self.sess.run(self.atrain_op, {self.tfs: s, self.tfa: a, self.tfadv: adv}) for _ in range(A_UPDATE_STEPS)]
        # update critic
        [self.sess.run(self.ctrain_op, {self.tfs: s, self.tfdc_r: r}) for _ in range(C_UPDATE_STEPS)]


    def _build_anet(self, name, trainable):
        with tf.variable_scope(name):
            l_1 = tf.layers.dense(self.tfs, 256, tf.nn.relu, trainable=trainable)
            a_prob = tf.layers.dense(l_1, A_DIM, tf.nn.softmax, trainable=trainable)
        params = tf.get_collection(tf.GraphKeys.GLOBAL_VARIABLES, scope=name)
        return a_prob, params

    def choose_action(self, s):
        prob_weights = self.sess.run(self.pi, feed_dict={self.tfs: s[None, :]})
        action = np.random.choice(range(prob_weights.shape[1]), p=prob_weights.ravel()) 
        tf.identity(prob_weights, name='probweights')
        logger.info("action:{1} prob:{0}".format(str(action), str(self.aloss)))
        return action

    def get_v(self, s):
        return self.sess.run(self.v, {self.tfs: s})[0, 0]

    def output_nodes(self):
        return ["state", "action", "advantage",  "critic/discounted_r", "probweights"]

    def freeze_graph(self):
        logger.info('**** Saved Model ****')
        self.saver.save(self.sess, self.model_path  + 'model.ckpt')
        # tf.train.write_graph(self.sess.graph_def, self.model_path, 'raw_graph.pbtxt',  as_text=True)
        tf.train.write_graph(self.sess.graph_def, self.model_path, 'raw_graph.pb',  as_text=False)

        checkpoint = tf.train.get_checkpoint_state(self.model_path)
        input_checkpoint = checkpoint.model_checkpoint_path
        output_graph = self.model_path + "ppo.bytes"
        clear_devices = True
        saver = tf.train.import_meta_graph(input_checkpoint + '.meta', clear_devices=clear_devices)
        graph = tf.get_default_graph()
        input_graph_def = graph.as_graph_def()

        with tf.Session() as sess:
            saver.restore(sess, input_checkpoint)
            output_graph_def = graph_util.convert_variables_to_constants(sess, input_graph_def, self.output_nodes()) 
     
            # Finally we serialize and dump the output graph to the filesystem
            with tf.gfile.GFile(output_graph, "wb") as f:
                f.write(output_graph_def.SerializeToString())
                # print(output_graph_def)
            logger.info("{0} ops in the final graph.".format(str(len(output_graph_def.node))))
 
 

