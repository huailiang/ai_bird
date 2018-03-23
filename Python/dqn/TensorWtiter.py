 # coding=utf8

"""
在tensorboard 记录一些参数信息
tensorboard 反应loss等的变化
"""

import numpy as np
import tensorflow as tf

class Writer():
    """docstring for Writer"""
    def __init__(self,_summary_writer,_sess):
        self.summary_writer = _summary_writer
        self.sess=_sess


    def write_text(self, key, input_dict, steps):
        """
        Saves text to Tensorboard.
        :param summary_writer: writer associated with Tensorflow session.
        :param key: The name of the text.
        """
        try:
            s_op = tf.summary.text(key,
                    tf.convert_to_tensor(([[str(x), str(input_dict[x])] for x in input_dict]))
                    )
            s = self.sess.run(s_op)
            self.summary_writer.add_summary(s, steps)
            self.summary_writer.flush()
        except Exception,e:
            print("Cannot write text summary for Tensorboard, error:{0}".format(e.message))
      

    def write_summary(self, key,value,step):
        summary = tf.Summary()
        summary.value.add(tag='Info/{}'.format(str(key)), simple_value=value)
        self.summary_writer.add_summary(summary, step)
        self.summary_writer.flush()


