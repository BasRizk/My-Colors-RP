# Module to handle Answers data compuations

import numpy as np
import matplotlib.pyplot as plt
import pandas as pd

def centroid(data):
    x, y = zip(*data)
    l = len(x)
    return sum(x) / l, sum(y) / l
    
def leave_it_for_now(filepath):
    
    
    recent_answers = pd.read_csv('past_answers.csv')
    recent_answers = recent_answers[recent_answers.iloc[:,4] == True]
    
    blue = recent_answers.iloc[:,3].values
    green = recent_answers.iloc[:,2].values
    red = recent_answers.iloc[:,1].values
    
    X = recent_answers.iloc[:, [1, 2]].values
    # Green & Red
    Y = recent_answers.iloc[:, [2, 3]].values
    # Red & Blue
    Z = recent_answers.iloc[:, [3, 1]].values
    
    bg = centroid(X)
    gr = centroid(Y)
    rb = centroid(Z)
    
    request_red = int((gr[1] + rb[0]) // 2)
    request_green = int((bg[1] + gr[0]) // 2)
    request_blue = int((bg[0] + rb[1]) // 2)


def get_centroid_of_answers(filepath):
    
    recent_answers = pd.read_csv(filepath)
    recent_answers = recent_answers[recent_answers.iloc[:,4] == True]
    
    X = recent_answers.iloc[:, [1, 2, 3]].values

    
    from sklearn.cluster import KMeans
    kmeans = KMeans(n_clusters = 1, init = 'k-means++', max_iter = 300, 
                    n_init = 10, random_state = 0)
    y_kmeans = kmeans.fit_predict(X)
        
    request_red = int(kmeans.cluster_centers_[:, 0][0])
    request_green = int(kmeans.cluster_centers_[:, 1][0])
    request_blue = int(kmeans.cluster_centers_[:, 2][0])
    
    return (request_red, request_green, request_blue)


