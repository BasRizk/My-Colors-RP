# -*- coding: utf-8 -*-
# A module to handle all Learning Colors data creations & manipulations
from ColorsRequests import list_of_colors
from DataManager import create_json_file, get_past_learned_colors_path
from BrainAnalysis import get_colors_by_analyzing_past_learning_data

import random
import os.path

def generate_learning_data(num_of_colors = 10, timeToLearn_default = 5):
    """
    :param num_of_colors - num of colors to learn to be created
    """

    print('Learning Data to be generated.')
    
    learning_data = {}
    learning_data['colorsToLearn'] = _list_of_learning_data(num_of_colors, timeToLearn_default)
    create_json_file('updated_learning_data.json', learning_data)
    
    print('LEARNING_GENERATION_COMPLETED')

def _list_of_learning_data(num_of_colors, timeToLearn_default):
    """
    Returns a list of 'ColorsToLearnData' depending on if there were past answers,
    so Colors created in a 'random-close' fashion according to past answers,
    or in a 'random' fashion if no past_answers_found.
    
    :param num_of_colors - Number of colors to learn
    """
    
    past_colors_path = get_past_learned_colors_path()

    if os.path.exists(past_colors_path):
        #colors_json = list_of_colors(num_of_colors, mechanism = 'eeg-reverse')
        print('Colors generation by "eeg-reverse" mechanism')
        colors_json = get_colors_by_analyzing_past_learning_data()
        random.shuffle(colors_json)
    else:
        print('Colors generation by "random" mechanism')
        colors_json = list_of_colors(num_of_colors, mechanism = 'random')
    
    if colors_json is not None:
        _make_ready_for_learning(colors_json, num_of_colors, timeToLearn_default)
    return colors_json

#### TODO ANALYSE AND ADD TIMING DIFFS
def _make_ready_for_learning(colors_json, num_of_colors, timeToLearn_default = 20):
    """
    Cleans a list of colors coming from the Colors-Names API to be ready to
    used for learning and later manipulations.
    
    :param colors_json - the list of colors in (Json) format
    """

    for i in range(num_of_colors):
        colors_json[i] = _construct_color_to_learn_data(colors_json[i], timeToLearn_default)

def _construct_color_to_learn_data(color_json, time_to_learn = 5):
    """
    :param color_json - (json) format color coming from Colors-Names API
    :param time_to_learn - the time for color to stay on screen
    """
    color_to_learn = {}
    color_to_learn['colorName'] = color_json['name']
    color_to_learn['colorRGB'] = [
            color_json['rgb']['r'],
            color_json['rgb']['g'], 
            color_json['rgb']['b']] 
    color_to_learn['timeToLearn'] = time_to_learn
    color_to_learn['colorHexInDec'] = int(color_json['hex'].replace("#", ""), 16)

    return color_to_learn