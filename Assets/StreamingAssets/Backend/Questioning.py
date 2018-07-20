# -*- coding: utf-8 -*-
# A module to hold Questioning creations
from ColorsRequests import get_random_colors
from DataManager import create_json_file, get_learning_data

import random

def generate_answers(question_data):
    """
    Appends to a cleaned question data its answers randomly including
    the correct_color data -correct answer- having the coming data clean
    for the question.

    :param question_data - cleaned question data
    """
    correct_color_name = question_data['colorName']
    correct_rgb = question_data['colorRGB']
    question_data['answers'] = []
    answer_data = {
            'colorName' : correct_color_name,
            'colorRGB'  : correct_rgb,
            'isCorrect' : True
            }
    question_data['answers'].append(answer_data)

    random_colors = get_random_colors(2)
    if random_colors is None:
        print ('[!] Request FAILED')
        return None
        
    for i in range(len(random_colors)):
        answer_data = _construct_answer_data(random_colors[i], isCorrect = False)
        question_data['answers'].append(answer_data)
        
    random.shuffle(question_data['answers'])
    
def _construct_answer_data(color_json, isCorrect = False):
    """
    Returns answer_data converted from a raw JSON formatted color coming from
    Colors-Names API.
    
    :param color_json - raw JSON formatted color
    :param isCorrect - correctness of the answer (default: False)
    """

    answer_data = {
                'colorName' : color_json['name'],
                'colorRGB'  : [color_json['rgb']['r'],
                               color_json['rgb']['g'],
                               color_json['rgb']['b']],
                'isCorrect' : isCorrect}
    return answer_data
 
def list_of_questions(colors_json, num_of_questions =-1):
    """
    Returns a list of questions based on given JSON format set of colors.
    
    :param colors_json - cleaned JSON format set of colors
    :param num_of_questions - num_of_questions to be created
        (default: same number of colors)
    """
    
    if num_of_questions <= 0:
        num_of_questions = len(colors_json)
    
    random.shuffle(colors_json)

    list_of_questions = []     
    for i in range(num_of_questions):
        question_data = _construct_question_data(colors_json[i])
        list_of_questions.append(question_data)

    return list_of_questions

def _construct_question_data(color_json):
    """
    Returns a question data from a JSON format of a cleaned color
    
    :param color_json - clean json formatted color
    """
    
    question_data = {}
    question_data['colorName'] = color_json['colorName']
    question_data['colorRGB'] = color_json['colorRGB']
    generate_answers(question_data)
    return question_data
    
def generate_questioning_data(num_of_questions = 10, time_limit = 20,
                        points_added_for_correct_answer = 10):
    """
    Generates a JSON file having questions based on recent Learned data.
    
    :param num_of_questions - number of questions to generate
    :param time_limit - time limit for counter (seconds)
    :param points_added_for_correct_answer
    """
    print('Next Round Data to be generated.')
    round_data = {}
    round_data['timeLimitInSeconds'] = time_limit
    round_data['pointsAddedForCorrectAnswer'] = points_added_for_correct_answer

    list_of_learning_data = get_learning_data()
    round_data['questions'] = list_of_questions(list_of_learning_data, num_of_questions)    
    create_json_file('updated_questions_data.json', round_data)
    print('QUESTIONING_GENERATION_COMPLETED')

    