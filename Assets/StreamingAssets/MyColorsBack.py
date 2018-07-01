from ColorsRequests import get_random_colors, random_close_list_of_colors
from FilesManager import get_app_path, create_json_file, get_op, get_learning_data

import random
import os.path
import AnswersAnalysis

def generate_answers(question_data):
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
        answer_data = construct_answer_data(random_colors[i], isCorrect = False)
        question_data['answers'].append(answer_data)
        
    random.shuffle(question_data['answers'])
    
def construct_answer_data(color_json, isCorrect = False):
    answer_data = {
                'colorName' : color_json['name'],
                'colorRGB'  : [color_json['rgb']['r'],
                               color_json['rgb']['g'],
                               color_json['rgb']['b']],
                'isCorrect' : isCorrect}
    return answer_data
    
def construct_question_data(color_json):
    question_data = {}
    question_data['colorName'] = color_json['colorName']
    question_data['colorRGB'] = color_json['colorRGB']
    generate_answers(question_data)
    return question_data

def list_of_colors(num_of_questions = 10, mechanism = 'random',
                      rgb = (128,128,128), distance = 15):
    
    if mechanism == 'random-close':
        colors_json = random_close_list_of_colors(rgb, distance, num_of_questions)
    else:   # random
        colors_json = get_random_colors(num_of_questions)

    if colors_json is None:
        print ('[!] Request FAILED')
        return None

    return colors_json

def list_of_questions(colors_json, num_of_questions =-1):
    if num_of_questions <= 0:
        num_of_questions = len(colors_json)
    
    random.shuffle(colors_json)

    list_of_questions = []     
    for i in range(num_of_questions):
        question_data = construct_question_data(colors_json[i])
        list_of_questions.append(question_data)

    return list_of_questions
    
def list_of_learning_data(num_of_colors):
    app_path = get_app_path() 
    past_answers_path = os.path.join(app_path, 'past_answers.csv')

    if os.path.exists(past_answers_path):
        centroid_of_answers =\
            AnswersAnalysis.get_centroid_of_answers(past_answers_path)
        colors_json = list_of_colors(num_of_colors, mechanism = 'random-close',
                             rgb = centroid_of_answers, distance = 15)
    else:
        colors_json = list_of_colors(num_of_colors, mechanism = 'random')

    make_ready_for_learning(colors_json)
    return colors_json

def construct_color_to_learn_data(color_json, time_to_learn_default = 5):
    color_to_learn = {}
    color_to_learn['colorName'] = color_json['name']
    color_to_learn['colorRGB'] = [
            color_json['rgb']['r'],
            color_json['rgb']['g'], 
            color_json['rgb']['b']] 
    color_to_learn['timeToLearn'] = time_to_learn_default

    return color_to_learn

#### TODO ANALYSE AND ADD TIMING
def make_ready_for_learning(colors_json):
    time_to_learn_default = 20
    for i in range(len(colors_json)):
        colors_json[i] = construct_color_to_learn_data(colors_json[i], time_to_learn_default)
    
def generate_questions_data(num_of_questions = 10, timeLimitInSeconds = 20,
                        pointsAddedForCorrectAnswer = 10):
                   
    round_data = {}
    round_data['timeLimitInSeconds'] = timeLimitInSeconds
    round_data['pointsAddedForCorrectAnswer'] = pointsAddedForCorrectAnswer

    list_of_learning_data = get_learning_data()['colorsToLearn']
    round_data['questions'] = list_of_questions(list_of_learning_data, num_of_questions)    
    create_json_file('updated_questions_data.json', round_data)

    
def generate_learning_data(num_of_colors = 10):
    learning_data = {}
    learning_data['colorsToLearn'] = list_of_learning_data(num_of_colors)
    create_json_file('updated_learning_data.json', learning_data)


# Running Program
operation = get_op()

if operation == 'GenerateLearningData':
    print('Learning Data to be generated.')
    generate_learning_data(num_of_colors = 5)
else: # GenerateQuestionsData
    print('Next Round Data to be generated')
    # Generating Round 1 Data randomly then each depending on previous
    generate_questions_data(num_of_questions = 2, timeLimitInSeconds = 20,
                pointsAddedForCorrectAnswer = 10)

