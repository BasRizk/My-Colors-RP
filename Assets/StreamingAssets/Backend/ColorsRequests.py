# -*- coding: utf-8 -*-
# A module to handle all of the Colors Requests and sub operations

from requests.adapters import HTTPAdapter
from urllib3.util import Retry
from RgbHex import rgb_to_hex

import json
import random
import requests


colors_api_url_online_base = 'https://api.color.pizza/v1/'
colors_api_url_base = 'http://localhost:8080/v1/'
headers = {'Content-Type': 'application/json'}

def _requests_retry_session(
    retries=3,
    backoff_factor=0.3,
    status_forcelist=(500, 502, 504),
    session=None,
):
    """
    Returns a session to be used for requesting from an API approriately,
    handling some kind of failures through retrying.
    """
    session = session or requests.Session()
    retry = Retry(
        total=retries,
        read=retries,
        connect=retries,
        backoff_factor=backoff_factor,
        status_forcelist=status_forcelist,
    )
    adapter = HTTPAdapter(max_retries=retry)
    session.mount('http://', adapter)
    session.mount('https://', adapter)
    return session

def _request_colors(colors_in_hex):
    """
    Returns json format having the requested HEX color data.
    
    :parm colors_in_hex - color in hex format; it has to be of 6 hex digits
    """
    
    s = requests.Session()
    s.headers.update(headers)
    api_url = ('{0}'+ colors_in_hex).format(colors_api_url_online_base)
    response = _requests_retry_session(session=s).get(api_url)

    """
    while True:
        response = requests.get(api_url, headers = headers)
        if response.status_code != 500:
            break
        else:
            # Hope it won't 500 a little later
            time.sleep(1)
    """
    
    #print('response: ', response.status_code)
    if response.status_code == 200:
        return json.loads(response.content.decode('utf-8'))
    else:
        return None

def get_colors_back(hexInDecColors):
    request_list = []
    for i in hexInDecColors:
        request_list.append(_validate_hex_color(i))
        
    predicted_colors = _request_colors(_colors_request_string(request_list))['colors']
    
    unique_predicted_colors = []
    for color_a in predicted_colors:
        color_added = False
        for color_b in unique_predicted_colors:
            if color_a['name'] == color_b['name']:
                color_added = True
                break
        if not color_added:
            unique_predicted_colors.append(color_a)
   
    return unique_predicted_colors

def _validate_hex_color(color_value):
    """
    Converts & validates a correct and clean hex value for colors
    
    :param color_value - decimal value for color
    """
    hexColorValue = str(hex(int(color_value))).replace('0x','')
    while(len(hexColorValue) != 6):
        hexColorValue = "0" + hexColorValue
    
    return hexColorValue

def list_of_colors(num_of_colors = 10, mechanism = 'random',
                      rgb = (128,128,128), distance = 15, existing_colors = None):
    """
    Returns a list of colors in json format coming from Colors-Nams API.
    
    :param num_of_colors - # of colors to be requested.
    :param mechanism - the type of algorithm to be used in selecting the colors
    it can be:
        - random-close; to a point with a max/min distance, or
        - random; totally random.
    :param rgb - RGB point as a tuple to be used in case of mechanism = 'random-close'
    :param distance - max/min distance to be used in case of mechanism = 'random-close'
    
    """
    if mechanism == 'random-close':
        colors_json = random_close_list_of_colors(rgb, distance, num_of_colors)
    elif mechanism == 'eeg-reverse':
        from BrainAnalysis import get_colors_by_brain_analysis
        colors_json = get_colors_by_brain_analysis(verbose=False)
    elif mechanism == 'random':
        colors_json = get_random_colors(num_of_colors)
    else:
        colors_json = get_other_than_colors(num_of_colors, existing_colors)

    if colors_json is None:
        print ('[!] Request FAILED')
        return None

    return colors_json

def get_other_than_colors(num_of_colors, existing_colors):
    """
    Returns random new set of colors other than the existing ones
    """
    
    if not existing_colors:
        return None
    existing_colors_hex_list = []
    for i in existing_colors:
        existing_colors_hex_list.append(hex(i['colorHexInDec']))
    new_colors_hex_list = []
    for i in num_of_colors:
        red = int(random.random()*256)
        green = int(random.random()*256)
        blue = int(random.random()*256)
        hex_color = rgb_to_hex(red, green, blue)
        if hex_color not in existing_colors_hex_list:
            new_colors_hex_list.append(hex_color)

    hex_colors_request = _colors_request_string(new_colors_hex_list)
    colors_json = _request_colors(hex_colors_request)

    if colors_json is not None:
        return colors_json['colors']
    return None

        

def random_close_list_of_colors(rgb, distance = 15, num_of_colors = 10):
    """
    Returns a random-close list of colors coming from Colors-Names API.
    
    :param rgb - RGB point as a tuple
    :param distance - max/min distance to be used.
    """
    hex_colors_list = []
    while len(hex_colors_list) < num_of_colors:
        close_rgb = _random_close_rgb(rgb, distance)
        hex_color = rgb_to_hex(close_rgb[0], close_rgb[1], close_rgb[2])
        if hex_color not in hex_colors_list:
            hex_colors_list.append(hex_color)
            
    hex_colors_request_string = _colors_request_string(hex_colors_list)
    colors_json = _request_colors(hex_colors_request_string)
    
    if colors_json is not None:
        return colors_json['colors']
        
    return None

def _random_close_rgb(rgb, distance = 15):
    """
    Returns a tuple of random close RGB value according to max/min distance.
    
    :param rgb - RGB point as a tuple
    :param distance - max_min distance to be used
    """
    r = _to_be_random_between(rgb[0], distance)
    g = _to_be_random_between(rgb[1], distance)
    b = _to_be_random_between(rgb[2], distance)
    return (r,g,b)

def _to_be_random_between(value, distance, minimum = 0, maximum = 255):
    """
    Returns a partly random value in a fixed range considering a distance,
    and not to pass a wider range of values
    """
    value = value + random.randint(-1*distance, distance) 
    if(value > maximum):
        value = maximum
    elif(value < 0):
        value = 0
    return value

def get_random_colors(num_of_colors = 10):
    """
    Returns a list of random colors in (JSON) format coming from Colors-Names API.
    """
    hex_colors_list = []
    while len(hex_colors_list) < num_of_colors:
        red = int(random.random()*256)
        green = int(random.random()*256)
        blue = int(random.random()*256)
        hex_color = rgb_to_hex(red, green, blue)
        if hex_color not in hex_colors_list:
            hex_colors_list.append(hex_color)

    hex_colors_request = _colors_request_string(hex_colors_list)
    colors_json = _request_colors(hex_colors_request)

    if colors_json is not None:
        return colors_json['colors']

    return None

def _colors_request_string(hex_colors_list):
    """
    Returns a the request string to be used in Colors-Names API converted from
    hex_colors_list.
    """
    colors_request_string = ''
    for i in hex_colors_list:
        colors_request_string += i + ","
    return colors_request_string

# Not Used
def _request_color_in_rgb(red, green ,blue):
    """
    Returns a color using RGB values only.
    ***to be used for quick testing***
    """
    hex_color = rgb_to_hex(red, green, blue)
    colors_json = _request_colors(hex_color)

    if colors_json is not None:
        return colors_json['colors']

    return None

