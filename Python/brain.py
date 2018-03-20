import numpy as np
import pandas as pd
import os

class RL(object):
    def __init__(self, _actions, _states, learning_rate=0.01, _gamma=0.9, _epsilon=0.9):
        self.actions = _actions 
        self.lr = learning_rate
        self.gamma = _gamma
        self.epsilon = _epsilon
        self.states = _states
        self.step = 0
        self.csv = "q_table.csv"
        self.state_num = len(self.states)
        self.action_num = len(self.actions)
        # if os.path.exists(self.csv):
        #     self.q_table = pd.read_csv(self.csv)
        #     print(" load q_table: "+str(self.q_table))
        # else:
        self.q_table = pd.DataFrame(np.zeros((self.state_num,self.action_num)), columns=self.actions, index = self.states)
        print self.q_table


    def choose_action(self, observation):
        # action selection
        if np.random.rand() < self.epsilon:
            # choose best action
            state_action = self.q_table.loc[observation, :]
            state_action = state_action.reindex(np.random.permutation(state_action.index))     # some actions have same value
            action = state_action.idxmax()
            print "state_action:"+str(state_action)+" action:"+str(action)+" state:"+str(observation)
        else:
            # choose random action
            action = np.random.choice(self.actions)

        self.step = self.step+1
        if(self.step%10==0):
            print self.q_table
        return action

    def learn(self, *args):
        pass

    def export(self):
        print "brain export"
        self.q_table.to_csv(self.csv)


# off-policy
class QLearningTable(RL):
    def __init__(self, actions, states, learning_rate=0.01, _gamma=0.9, _epsilon=0.9):
        super(QLearningTable, self).__init__(actions,states, learning_rate, _gamma, _epsilon)

    def learn(self, s, a, r, s_):
        q_predict = self.q_table.loc[s, a]
        if s_ != 'terminal':
            q_target = r + self.gamma * self.q_table.loc[s_, :].max()  # next state is not terminal
        else:
            q_target = r  # next state is terminal
        self.q_table.loc[s, a] += self.lr * (q_target - q_predict)  # update


# on-policy
class SarsaTable(RL):

    def __init__(self, actions, states, learning_rate=0.01, _gamma=0.9, _epsilon=0.9):
        super(SarsaTable, self).__init__(actions, states, learning_rate, _gamma, _epsilon)

    def learn(self, s, a, r, s_, a_):
        q_predict = self.q_table.loc[s, a]
        if s_ != 'terminal':
            q_target = r + self.gamma * self.q_table.loc[s_, a_]  # next state is not terminal
        else:
            q_target = r  # next state is terminal
        self.q_table.loc[s, a] += self.lr * (q_target - q_predict)  # update


