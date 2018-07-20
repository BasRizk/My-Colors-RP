# -*- coding: utf-8 -*-

from DataManager import get_op
from Questioning import generate_questioning_data
from Learning import generate_learning_data

# Running Program
operation = get_op()

if operation == 'GenerateLearningData':
    num_of_colors_to_learn = 3
    time_to_learn = 5
    generate_learning_data(num_of_colors = num_of_colors_to_learn, timeToLearn_default = time_to_learn)
    
else: # GenerateQuestionsData
    generate_questioning_data(num_of_questions = 2, time_limit = 20,
                points_added_for_correct_answer = 10)

