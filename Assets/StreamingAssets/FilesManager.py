# A module to handle all of file operations except csv files

import os.path
import json
import sys


def create_json_file(filename, data):
    data_path = os.path.join(get_app_path(), filename)

    with open(data_path, 'w') as outfile:
        json.dump(data, outfile)
        return True
    return False

def get_app_path():
    try:
        # app_path = os.path.abspath(os.path.dirname(__file__))
        app_path = sys.argv[1]
    except:
        app_path = ""
    return app_path

def get_op():
    try:
        op = sys.argv[2]
    except:
        op = ""
    return op

def get_learning_data():
    app_path = get_app_path() 
    learning_data_path = os.path.join(app_path, 'updated_learning_data.json')
    
    return json.loads(open(learning_data_path).read()) 
    
