# -*- coding: utf-8 -*-
# A module to handle all Brain singals aquisation and manipulation analysis

import numpy as np
import matplotlib.pyplot as plt
import statsmodels.formula.api as sm
import sys

from DataManager import get_learning_raw_data, get_brain_signals_data
from ColorsRequests import get_colors_back  

def analyze_past_learning_data(verbose = False):
    
    recent_signals = get_brain_signals_data()
    recent_learned_colors = get_learning_raw_data()
        
    X, y = match_signals_as_colors(recent_signals, recent_learned_colors, signals_format='CyKIT')
    y  = y.reshape(-1,1)
    
    X = backward_eliminate_features(X, y, 0.05, 'pValues', verbose)
    
    # Feature Scaling
    from sklearn.preprocessing import StandardScaler
    sc_X = StandardScaler()
    X = sc_X.fit_transform(X)
    sc_y = StandardScaler()
    y = sc_y.fit_transform(y)

    # Fitting the Classifier to the recent_signals & recent_learned_colors
    # after feature scaling and backward elimination
    # Notes:
    # - nu = 5/len(x) supposed to give 5 support_vectors but that does not happen
    #   as their are some boundaries, but it still decreases the number of support_vectors
    #   to a good value
    from sklearn.svm import NuSVR
    regressor = NuSVR(kernel = 'rbf', verbose = True)
    regressor.fit(X, y)

    support_vectors = regressor.support_vectors_
    
    if verbose:      
        # TODO -- Fitting hierarichal clustering to the support_vectors of
        # the past Support Vector Regressor to biggest cluseters (TODO yet)
        # Now just for acknowledgement
        from sklearn.cluster import AgglomerativeClustering
        hc = AgglomerativeClustering(n_clusters = 2, affinity = 'euclidean',
                                     linkage = 'ward')
        y_hc = hc.fit_predict(support_vectors)
          
        visualizing_the_clusters(regressor, support_vectors, y_hc, len(support_vectors[-1]))
        
    return regressor, sc_X, sc_y, recent_signals, recent_learned_colors

def get_colors_by_brain_analysis(verbose=False):
    # Applying some random function to some feather blowing on their values to
    # flat_for(support_vectors, lambda x: x + random.randint(-1*int(x*0.1),int(x*0.1)))
    # predict after that a new set of colors
    
    # TODO use the mode reverse distribution algorithm after and verbose
    # the opposition of values on graph
    regressor, sc_X, sc_y, recent_signals, recent_learned_colors = analyze_past_learning_data(verbose)
    support_vectors = regressor.support_vectors_
    
    predictions = sc_y.inverse_transform(regressor.predict(support_vectors))
    original_predicted_colors = get_colors_back(predictions)
    
    reversed_support_vectors = reverse_distribution(support_vectors, verbose)
    reversed_support_vectors_cleared =\
        remove_redundancies_with(reversed_support_vectors, support_vectors)
    predictions = sc_y.inverse_transform(regressor.predict(reversed_support_vectors_cleared))
    reverse_predicted_colors = get_colors_back(predictions)
    
    new_balancing_colors = remove_color_redundancies_with(reverse_predicted_colors, original_predicted_colors)
    
    if verbose:
        past_colors = get_colors_back(recent_learned_colors)
        print('Past Colors were: \n', past_colors)
        print(' \n SVR duel_coef_ = ',regressor.dual_coef_,
              ' support_ = ',regressor.support_,
              ' SVR intercept_ = ',regressor.intercept_)
    
    if len(new_balancing_colors) < 6:
        print('Balancing Colors observed seems to be very few')
        new_balancing_colors = reverse_predicted_colors
    
    return new_balancing_colors

def remove_color_redundancies_with(a, b):
    no_redunancies_here = []
    for i in a:
        found_in_b = False
        for j in b:
            if i['name'] == j['name']:
                found_in_b = True
                break
        if not found_in_b:
            no_redunancies_here.append(i)
    return no_redunancies_here

def remove_redundancies_with(a, b):
    """
    Returns list a removed from it similarities with list b
    """
    
    no_redunancies_here = []
    for i in a:
        if i not in b:
            no_redunancies_here.append(i)
    return no_redunancies_here

def reverse_distribution(X, verbose=False):
    """
    Returns 2d array X after applying Reverse Distribution Algorithm on
    the following 2d array X
    """
    X_to_be_used = X
    reversed_vectors = []
    
    if verbose:
        print('\n')
    num_of_vectors = len(X_to_be_used)
    for i in range(num_of_vectors):
        reversed_vectors.append(_reverse_distribute_1d_array(X_to_be_used[i]))
        if verbose:
            sys.stdout.write("Reverse Distribution Progress: %d%% \r" % ((i//num_of_vectors)*100) )
            sys.stdout.flush()
            print('\n')
        
    return np.array(reversed_vectors).reshape(-1,len(X_to_be_used[0]))

def _reverse_distribute_1d_array(array_1d):
    """
    Returns array_1d after applying Reverse Distribution algorithm on it
    """
    vector_len = len(array_1d)
    org_vector = array_1d
    org_num_vector = np.zeros(vector_len)
    for i in range(vector_len):
        value_a = org_vector[i]
        its_number = 0
        for j in range(vector_len):
            value_b = org_vector[j]
            if value_a > value_b:
                #print('value_a ', value_a, ' value_b ', value_b)
                its_number+=1
        org_num_vector[i] = its_number

    reversed_vector = org_vector.copy()
    max_num = vector_len -1
    min_num = 0
    is_reversed_vector = [False]*vector_len
    for i in range(vector_len):
        if not is_reversed_vector[i]:
            
            selected_num = org_num_vector[i]
            
            if selected_num == min_num:
                index_to_get = _get_index_of(org_num_vector, max_num)                        
            elif selected_num == max_num:
                index_to_get = _get_index_of(org_num_vector, min_num)   
                     
            else:
                if max_num - selected_num < selected_num - min_num:
                    num_to_get = min_num + (max_num - selected_num)
                elif max_num - selected_num > selected_num - min_num:
                    num_to_get = max_num - (selected_num - min_num)
                else:
                    num_to_get = max_num//2                   
                index_to_get = _get_index_of(org_num_vector, num_to_get)
                
            tmp_value = reversed_vector[i]    
            reversed_vector[i] = org_vector[index_to_get]
            reversed_vector[index_to_get] = tmp_value
            is_reversed_vector[i] = True
            is_reversed_vector[index_to_get] = True

    return reversed_vector


def _get_index_of(array, num_to_get):

    while num_to_get > 0:
        for index_to_get in range(len(array)):
            if array[index_to_get] == num_to_get:
                return index_to_get
        num_to_get-=1
    return 0
    
def flat_for(np_array, func):
    """
    Applies function (func) on an numpy array
    """
    
    np_array = np_array.reshape(-1)
    for i, v in enumerate(np_array):
        np_array[i] = func(v)

def unique_rows(array):
    """
    Returns an array with rows of no duplicates
    """
    
    array = np.ascontiguousarray(array)
    unique_a = np.unique(array.view([('', array.dtype)]*array.shape[1]))
    return unique_a.view(array.dtype).reshape((unique_a.shape[0], array.shape[1]))

def match_signals_as_colors(recent_signals, recent_learned_colors, signals_format = 'CyKIT'):
    """
    Returns X, y refering to recent_signals & recent_learned_colors respectivly
    to match each color with one one value of signal using mean value mechanism.
    """
    
    readings_per_color = len(recent_signals)//len(recent_learned_colors)

    if signals_format == 'CyKIT':    
        prev_X = recent_signals.iloc[:,[i for i in range(2,16)]].values
    else: # used_format = 'EmoKit'        
        prev_X = recent_signals.iloc[:,[(i+1) for i in range(0,28,2)]].values


    if len(prev_X) > readings_per_color*len(recent_learned_colors):
        prev_X = prev_X[:-(len(prev_X) - (readings_per_color*len(recent_learned_colors))),:]
        
    X = []
    for channel_num in range(14):
        sub_X = []
        for offset in range(0, len(recent_signals) - readings_per_color, readings_per_color):
            sub_X.append(np.mean(prev_X[offset: offset+readings_per_color,channel_num]))
        X.append(sub_X)
    X = np.transpose(np.array(X))
    y = recent_learned_colors
    
    return X,y

def match_colors_as_signals(recent_signals, recent_learned_colors, signals_format = 'CyKIT'):
    """
    Returns X, y refering to recent_signals & recent_learned_colors respectivly
    to match each signal with one color value using repeated value mechanism
    """
    
    readings_per_color = len(recent_signals)//len(recent_learned_colors)
    
    if signals_format == 'CyKIT':    
        x = recent_signals.iloc[:,[i for i in range(2,16)]].values
    else: # used_format = 'EmoKit'        
        x = recent_signals.iloc[:,[(i+1) for i in range(0,28,2)]].values

    y = np.repeat(recent_learned_colors, readings_per_color, axis = 0)
    
    if len(x) > len(y):
        x = x[:-(len(x) - len(y)),:]
        
    return x,y

def backward_eliminate_features(x, y, sl, mode='pValues+rSquared', verbose=False):
    """
    Return the features variable, after optimization using Backward Elimination
    
    -- Statically Done here --> to be automated
    
    :param x - features variable
    :param y - output variable
    :param sl - maximum allowed p-value
    :param mode - mode to use into consideration (pValues or pValues+rSquared)
    """    

    if mode == 'pValues+rSquared':
        return _backward_eliminate_using_pValues_rSquared(x, y, sl, verbose)
    else:   # mode = 'p-values'
        return _backward_eliminate_using_pValues(x, y, sl,verbose)
        
def _backward_eliminate_using_pValues(x, y, sl, verbose=False):
    numVars = len(x[0])
    for i in range(0, numVars):
        regressor_OLS = sm.OLS(y, x).fit()
        maxVar = max(regressor_OLS.pvalues).astype(float)
        if maxVar > sl:
            for j in range(0, numVars - i):
                if (regressor_OLS.pvalues[j].astype(float) == maxVar):
                    x = np.delete(x, j, 1)
        else:
            break
    if verbose:
        print(regressor_OLS.summary())
    return x

def _backward_eliminate_using_pValues_rSquared(x, y, SL, verbose=False):
    
    numVars = len(x[0])
    temp = np.zeros((len(x),numVars)).astype(int)
    for i in range(0, numVars):
        regressor_OLS = sm.OLS(y, x).fit()
        maxVar = max(regressor_OLS.pvalues).astype(float)
        adjR_before = regressor_OLS.rsquared_adj.astype(float)
        if maxVar > SL:
            for j in range(0, numVars - i):
                if (regressor_OLS.pvalues[j].astype(float) == maxVar):
                    temp[:,j] = x[:, j]
                    x = np.delete(x, j, 1)
                    tmp_regressor = sm.OLS(y, x).fit()
                    adjR_after = tmp_regressor.rsquared_adj.astype(float)
                    if (adjR_before >= adjR_after):
                        x_rollback = np.hstack((x, temp[:,[0,j]]))
                        x_rollback = np.delete(x_rollback, j, 1)
                        #print (regressor_OLS.summary())
                        return x_rollback
                    else:
                        continue
                    
    if verbose:        
        print(regressor_OLS.summary())
    return x
        
def visualizing_the_clusters(regressor, support_vectors, y_hc, num_of_features):
    
    # TODO based on num_of_features
    num_of_graphs = len(support)
    plt.subplot(2, 2, 1)
    plt.scatter(support_vectors[y_hc == 0, 0], regressor.predict(support_vectors[y_hc == 0,:]), s = 10,
                c = 'red', label = 'Clustering_0')
    plt.scatter(support_vectors[y_hc == 1, 0], regressor.predict(support_vectors[y_hc == 1, :]), s = 10,
                c = 'blue', label = 'Clustering_1')
    plt.title('Clusters of Signals')
    plt.xlabel('Signal 0')
    plt.ylabel('Color Value predicted')

    plt.subplot(2, 2, 2)
    plt.scatter(support_vectors[y_hc == 0, 1], regressor.predict(support_vectors[y_hc == 0,:]), s = 10,
                c = 'red', label = 'Clustering_0')
    plt.scatter(support_vectors[y_hc == 1, 1], regressor.predict(support_vectors[y_hc == 1, :]), s = 10,
                c = 'blue', label = 'Clustering_1')
    plt.title('Clusters of Signals')
    plt.xlabel('Signal 1')
    plt.ylabel('Color Value predicted')

    plt.subplot(2, 2, 3)    
    plt.scatter(support_vectors[y_hc == 0, 2], regressor.predict(support_vectors[y_hc == 0,:]), s = 10,
                c = 'red', label = 'Clustering_0')
    plt.scatter(support_vectors[y_hc == 1, 2], regressor.predict(support_vectors[y_hc == 1, :]), s = 10,
                c = 'blue', label = 'Clustering_1')    
    plt.title('Clusters of Signals')
    plt.xlabel('Signal 2')
    plt.ylabel('Color Value predicted')
    
    plt.subplot(2, 2, 4)
    plt.scatter(support_vectors[y_hc == 0, 3], regressor.predict(support_vectors[y_hc == 0,:]), s = 10,
                c = 'red', label = 'Clustering_0')
    plt.scatter(support_vectors[y_hc == 1, 3], regressor.predict(support_vectors[y_hc == 1, :]), s = 10,
                c = 'blue', label = 'Clustering_1')
    plt.title('Clusters of Signals')
    plt.xlabel('Signal 3')
    plt.ylabel('Color Value predicted')
    
    plt.legend()
    plt.show()