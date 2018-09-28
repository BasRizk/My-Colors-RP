# -*- coding: utf-8 -*-
"""
# A module to handle data & files basic operations
"""

import os.path
import json
import sys
import glob
import pandas as pd
import numpy as np
import csv

def get_vectors(vectors_type = 'reversed-vectors'):
    data_path = _create_data_path("")
    if vectors_type == 'reversed-vectors':
        allFiles = glob.glob(data_path + "/reversed_vectors_*.csv")
    else:   # support_vectors
        allFiles = glob.glob(data_path + "/support_vectors_*.csv")
        
    allFiles.sort()
    frame = pd.DataFrame()
    list_ = []
    for file_ in allFiles:
        print('Reading file ..', file_, '..')
        df = pd.read_csv(file_, index_col=None, skiprows=[], header=None)
        list_.append(df)
    frame = pd.concat(list_, sort=True)
    return frame

def document_recent_file_with(name, data):
    
    file_num = 0
    filename = name + "_" + file_num + ".csv"
    data_path = _create_data_path(name + "_" + file_num + ".csv")
    while(os.path.exists(data_path)):
        file_num +=1
        filename = name + "_" + file_num + ".csv"
        data_path = _create_data_path(name + "_" + file_num + ".csv")
        
    return create_csv_file(filename, data)

def create_csv_file(filename, data):
    data_path = _create_data_path(filename)

    with open(data_path, 'w') as outfile:
        writer = csv.DictWriter(outfile, data[0])
        writer.writerows(data[1:])
        return True
    return False

def create_json_file(filename, data):
    """
    Creates a (JSON) file.

    :param filename - filename of the file to be created
    :param data - json data to be hold into the file
    """
    data_path = _create_data_path(filename)

    with open(data_path, 'w') as outfile:
        json.dump(data, outfile)
        return True
    return False

def append_to_json_file(filename, data):
    data_path = _create_data_path(filename)
    past_data = json.loads(open(data_path).read())
    #print('past_data = ', past_data)
    past_data.append(data)
    for very_recent_color in data:
        past_data.append(very_recent_color)
    #print('all_data = ', past_data)
    create_json_file(filename, past_data)

def _get_app_path():
    """
    Returns the path of the running application.
    """
    try:
        # app_path = os.path.abspath(os.path.dirname(__file__))
        app_path = sys.argv[1]
    except:
        app_path = ".."
    return app_path
        
def get_op():
    """
    Returns the operation of the requested operation by the main program
    """
    try:
        op = sys.argv[2]
    except:
        op = ""
    return op

def get_learning_raw_data(mode = 'past_colors'):
    if mode == 'past_colors':
        data=np.array([
                np.array(color['colorHexInDec'])
                for color in get_past_colors()
                ])
        data = data[:-1]
    else:
        data=np.array([
                np.array(color['colorHexInDec'])
                for color in get_learning_data()
                ])
    return data


def get_learning_data():
    """
    Returns a json format string read from the last updated learning data file.
    """
    data_path = _create_data_path('updated_learning_data.json')
    return json.loads(open(data_path).read())['colorsToLearn']
    
def get_past_colors():
    data_path = _create_data_path('past_colors.json')
    return json.loads(open(data_path).read())

def get_brain_signals_data():
    data_path = _get_brain_signals_data_path()
    allFiles = glob.glob(data_path + "/*.csv")
    allFiles.sort()
    allFiles = allFiles[:]
    frame = pd.DataFrame()
    list_ = []
    for file_ in allFiles:
        print('Reading file ..', file_, '..')
        df = pd.read_csv(file_, index_col=None, skiprows=[0], header=None)
        list_.append(df)
    frame = pd.concat(list_, sort=True)
    return frame

def append_to_past_colors(colors_json, past_colors_exist):
    if not past_colors_exist:
        create_json_file('past_colors.json', colors_json)
    else:
        append_to_json_file('past_colors.json', colors_json)
    
def _get_brain_signals_data_path():
    return _create_data_path('EEG-Logs')

def _create_data_path(file_or_foldername):
    data_folder = _ensure_dir(os.path.join(_get_app_path(), 'Data'))
    pathCreated = os.path.join(data_folder, file_or_foldername)
    print('Data path created == ', pathCreated)
    return pathCreated

def get_past_learned_colors_path():
    return _create_data_path('past_colors.json')

def _ensure_dir(file_path):
    directory = os.path.dirname(file_path)
    if not os.path.exists(directory):
        os.makedirs(directory)
    return file_path
