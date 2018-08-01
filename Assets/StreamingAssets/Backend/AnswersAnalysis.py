# -*- coding: utf-8 -*-
"""
# Module to handle Answers data compuations
"""

import pandas as pd

def get_centroid_of_answers(filepath):
    """
    Returns the RGB centroid of a list of previous answers.
    
    :param filepath - filepath of the past_answers_file
    """
    
    recent_answers = pd.read_csv(filepath)
    recent_answers = recent_answers[recent_answers.iloc[:,4] == True]
    
    X = recent_answers.iloc[:, [1, 2, 3]].values

    
    from sklearn.cluster import KMeans
    kmeans = KMeans(n_clusters = 1, init = 'k-means++', max_iter = 300, 
                    n_init = 10, random_state = 0)
    kmeans.fit_predict(X)
        
    request_red = int(kmeans.cluster_centers_[:, 0][0])
    request_green = int(kmeans.cluster_centers_[:, 1][0])
    request_blue = int(kmeans.cluster_centers_[:, 2][0])
    
    return (request_red, request_green, request_blue)


