#!/usr/bin/env python

from setuptools import setup, Command, find_packages


with open('requirements.txt') as f:
    required = f.read().splitlines()

setup(name='bird',
      version='1.0.1',
      description='Machine Learning by PengHuailiang',
      license='MIT',
      author='PengHuailiang',
      author_email='peng_huailiang@qq.com',
      url='https://huailiang.github.io',
      packages=find_packages(exclude = ['models']),
      install_requires = required,
      long_description= ("Machine Learning allows researchers and developers "
       "to transform games and simulations created using the Unity Editor into environments "
       "where intelligent agents can be trained using reinforcement learning, evolutionary " 
       "strategies, or other machine learning methods through a simple to use Python API.")
     )
