# -*- coding: utf-8 -*-

from DataManager import get_op
from Questioning import generate_questioning_data
from Learning import generate_learning_data
from BrainAnalysis import calculate_errors

# Running Program
operation = get_op()

if operation == 'GenerateLearningData':
    num_of_colors_to_learn = 3
    time_to_learn = 30
    generate_learning_data(num_of_colors = num_of_colors_to_learn, timeToLearn_default = time_to_learn)
    
elif operation == 'GenerateColorsQuestions': # GenerateQuestionsData
    generate_questioning_data(num_of_questions = 5, time_limit = 20,
                points_added_for_correct_answer = 10, mode = 'Color-Names')
    
elif operation == 'GenerateMemoryQuestions': # GenerateMemoryQuestions
    generate_questioning_data(num_of_questions = 5, time_limit = 20,
                points_added_for_correct_answer = 10, mode = 'Memory')
elif operation == 'CalculateErrors': # CalculateErrors
    calculate_errors()