# -*- coding: utf-8 -*-
"""
# A module to hold Questioning creations
"""

from ColorsRequests import get_random_colors
from DataManager import create_json_file, get_past_colors

import random

def generate_questioning_data(num_of_questions = 10, time_limit = 20,
                              points_added_for_correct_answer = 10, mode):
    """
    Generate a JSON file having questions of type memory based on recent
    Learned data.
    
    :param num_of_questions - number of questions to generate
    :param time_limit - time limit for counter (seconds)
    :param points_added_for_correct_answer
    :param mode - (Memory - Color-Names)
    """
    print('Next Set of Questions to be generated.')
    questioning_data = {}
    questioning_data['timeLimitInSeconds'] = time_limit
    questioning_data['pointsAddedForCorrectAnswer'] = points_added_for_correct_answer
    
    list_of_learning_data = get_past_colors()
    
    print('Questions Mode = ', mode)
    if mode == "Memory":
        questioning_data['questions'] = _list_of_memory_questions(list_of_learning_data, num_of_questions, 0.5)    
    else: # Color-Names
        mode = "Color-Names"
        questioning_data['questions'] = _list_of_color_names_questions(list_of_learning_data, num_of_questions)    

    create_json_file('updated_questions_data.json', questioning_data)
        
    print('QUESTIONING_GENERATION_COMPLETED')
    
def _list_of_memory_questions(colors_json, num_of_questions, ratio):
    """
    Returns a list of Memory questions based on given JSON format set of colors.
    
    :param colors_json - cleaned JSON format set of colors
    :param num_of_questions - num_of_questions to be created
        (default: same number of colors)
    """
    
    if num_of_questions <= 0:
        num_of_questions = len(colors_json)
        
    num_of_correct = num_of_questions*ratio
    
    random.shuffle = len(colors_json)
    
    list_of_questions = []
    for i in range(num_of_correct):
        question_data = _construct_memory_question_data(color_json[i], isCorrect = True)
        list_of_questions.append(question_data)
    for i in range(num_of_questions - num_of_correct):
        weird_color_json = getRandomColors(5)
        question_data = _construct_memory_question_data(weird_color_json, isCorrect = False)
        
def _construct_memory_question_data(color_json, isCorrect):
    return {'colorName': colors_json['colorName'],
            'colorRGB' : colors_json['colorRGB'],
            'isCorrect' : isCorrect }
     
def _list_of_colors_names_questions(colors_json, num_of_questions =-1, mode):
    """
    Returns a list of Color-Names questions based on given JSON format set of colors.
    
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

def _construct_color_names_question_data(color_json):
    """
    Returns a question data from a JSON format of a cleaned color
    
    :param color_json - clean json formatted color
    """
    
    question_data = {}
    question_data['colorName'] = color_json['colorName']
    question_data['colorRGB'] = color_json['colorRGB']
    
    _generate_color_names_answers(question_data)
    return question_data

    
def _generate_color_names_answers(question_data):
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
    
def _construct_color_names_answer_data(color_json, isCorrect = False):
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
    