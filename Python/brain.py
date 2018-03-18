class BrainInfo:
    def __init__(self, observation, state, reward=None, local_done=None, action =None):
        """
        Describes experience at current step of all agents linked to a brain.
        """
        self.observations = observation
        self.states = state
        self.rewards = reward
        self.local_done = local_done
        self.previous_actions = action
