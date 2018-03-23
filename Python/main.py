# coding=utf8


"""
第一行放开：运行的是q_learing 环境
第二行放开：运行的是deep_q_network环境
"""

# from environment import UnityEnvironment
from dqn_environment import UnityEnvironment


env=UnityEnvironment("bird")


print str(env)



