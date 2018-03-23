#coding=utf8
import numpy as np
import tensorflow as tf

# 返回沿轴axis最大值的索引
state=[-1.8321208,-1.2807886]
state = np.array(state)
st = np.argmax(state)
print state,type(state),st
state = state[np.newaxis,]
st = np.argmax(state)
print state,type(state),st

x=[[1.,2.],[3.,4.]]
x_=tf.constant(x,shape=[2,2])

with tf.Session() as sess:
	y1_ = tf.reduce_mean(x_)
	y2_ =tf.reduce_mean(x_,0)
	y3_=tf.reduce_mean(x_,1)
	print sess.run(y1_),sess.run(y2_),sess.run(y3_)
	y1_=tf.reduce_max(x_)
	y2_=tf.reduce_max(x_,0)
	y3_=tf.reduce_max(x_,1)
	print sess.run(y1_),sess.run(y2_),sess.run(y3_)

# a=np.array(a)

# print a,type(a)

# print np.newaxis

# a=a[np.newaxis]

# print a


