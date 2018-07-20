# -*- coding: utf-8 -*-
# A module to handle data & files basic operations

import os.path
import json
import sys
import glob
import pandas as pd
import numpy as np
import csv

def create_csv_file(filename, data):
    data_path = create_data_path(filename)

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
    data_path = create_data_path(filename)

    with open(data_path, 'w') as outfile:
        json.dump(data, outfile)
        return True
    return False

def get_app_path():
    """
    Returns the path of the running application.
    """
    try:
        # app_path = os.path.abspath(os.path.dirname(__file__))
        app_path = sys.argv[1]
    except:
        app_path = ".."
    return app_path

def ensure_dir(file_path):
    directory = os.path.dirname(file_path)
    if not os.path.exists(directory):
        os.makedirs(directory)
    return file_path
        
def get_op():
    """
    Returns the operation of the requested operation by the main program
    """
    try:
        op = sys.argv[2]
    except:
        op = ""
    return op

def create_data_path(file_or_foldername):
    return os.path.join(get_data_folder(), file_or_foldername)

def create_path(file_or_foldername):
    return os.path.join(get_app_path(), file_or_foldername)


def get_data_folder():
    return ensure_dir(os.path.join(get_app_path(), 'Data'))

def get_past_learned_colors_path():
    return create_data_path('past_colors.csv')

def get_learning_data():
    """
    Returns a json format string read from the last updated learning data file.
    """
    data_path = create_data_path('updated_learning_data.json')
    return json.loads(open(data_path).read())['colorsToLearn']

def get_learning_raw_data():
    data=np.array([
        np.array(color['colorHexInDec'])
        for color in get_learning_data()
        ])
    return data

def get_brain_signals_path():
    return create_data_path('EEG-Logs')

def get_brain_signals_data():
    data_path = get_brain_signals_path()
    allFiles = glob.glob(data_path + "/*.csv")
    frame = pd.DataFrame()
    list_ = []
    for file_ in allFiles:
        print('Reading file ..', file_, '..')
        df = pd.read_csv(file_, index_col=None, skiprows=[0], header=None)
        list_.append(df)
    frame = pd.concat(list_, sort=True)
    return frame