<p align="center">
    <a href="https://huailiang.github.io/">
    	<img src="https://avatars0.githubusercontent.com/u/12636841?s=460&v=4" width="320" height="300">
    </a>
</p>

<b>强化学习 游戏AI Trainning的代码工程</b>


分为三个过程，对应到项目中都有不同的设置：

1.  <a href="https://huailiang.github.io/2018/03/19/reinforcement/">Unity 中应用Reinforcement-Q_Learning </a>

2.  <a href="https://huailiang.github.io/2018/03/20/reinforcement2/">外部环境(Python)实现游戏训练，Unity负责表现 </a>

3.  <a href="https://huailiang.github.io/2018/03/23/reinforcement3/">使用Deep Q Network 神经经网络训练游戏AI </a>

4.  <a href="https://huailiang.github.io/2018/11/10/ppo//">使用Policy Gradient & PPO神经经网络训练游戏AI </a>


## Shell

你可以查看此工程可以通过在ternimal输入下面命令：

```shell

git clone https://github.com/huailiang/bird

#切换到PolicyGradient
git checkout PolicyGradient

#切换到mulstate分支
git checkout mulstate

```


## Requirement

server:
- tensorflow==1.12.0
- matplotlib
- numpy>=1.11.0

client:
- unity >=2018.2.7
- <a href="https://s3.amazonaws.com/unity-agents/TFSharpPlugin.unitypackage">TFSharpPlugin</a>


## Mode

### play game by operation

![](/image/5.jpg)

每点击一次屏幕，小鸟就上飞一次，通过所有关卡

Unity Player Setting Symbols set as:

![](/image/6.jpg)

### train in python 

GameManager(c#) set External 

![](/image/2.jpg)

environment(python) set Train = True

![](/image/3.jpg)

### test in python

as opration like train in python and set Train = False

### test in unity

download TFSharpPlugin and import TFSharpPlugin to unity at first

Unity Player Setting Symbols set as:

![](/image/4.jpg)

GameManager(c#) set Internal and drag tensorflow output to Graph model

![](/image/1.jpg)


## Train

- Ensure Env as requirement

- Start with python main.py

- Unity Gamemanager mode set external 

- Start Unity


欢迎关注作者博客：https://huailiang.github.io
