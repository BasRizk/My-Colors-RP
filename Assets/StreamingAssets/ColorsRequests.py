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

def requests_retry_session(
    retries=3,
    backoff_factor=0.3,
    status_forcelist=(500, 502, 504),
    session=None,
):
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


def request_colors(colors_in_hex):
    
    s = requests.Session()
    s.headers.update(headers)
    api_url = ('{0}'+ colors_in_hex).format(colors_api_url_online_base)
    response = requests_retry_session(session=s).get(api_url)

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

def colors_request_string(hex_colors_list):
    colors_request_string = ''
    for i in hex_colors_list:
        colors_request_string += i + ","
    return colors_request_string

def get_random_colors(num_of_colors = 10):
    hex_colors_list = []
    while len(hex_colors_list) < num_of_colors:
        red = int(random.random()*256)
        green = int(random.random()*256)
        blue = int(random.random()*256)
        hex_color = rgb_to_hex(red, green, blue)
        if hex_color not in hex_colors_list:
            hex_colors_list.append(hex_color)

    hex_colors_request = colors_request_string(hex_colors_list)
    colors_json = request_colors(hex_colors_request)

    if colors_json is not None:
        return colors_json['colors']

    return None

    
def random_close_list_of_colors(rgb, distance = 15, num_of_colors = 10):
    hex_colors_list = []
    while len(hex_colors_list) < num_of_colors:
        close_rgb = random_close_rgb(rgb, distance)
        hex_color = rgb_to_hex(close_rgb[0], close_rgb[1], close_rgb[2])
        if hex_color not in hex_colors_list:
            hex_colors_list.append(hex_color)
            
    hex_colors_request_string = colors_request_string(hex_colors_list)
    colors_json = request_colors(hex_colors_request_string)
    
    if colors_json is not None:
        return colors_json['colors']
        
    return None

def random_close_rgb(rgb, distance = 15):
    r = to_be_random_between(rgb[0], distance)
    g = to_be_random_between(rgb[1], distance)
    b = to_be_random_between(rgb[2], distance)
    return (r,g,b)

def to_be_random_between(value, distance, minimum = 0, maximum = 255):
    value = value + random.randint(-1*distance, distance) 
    if(value > maximum):
        value = maximum
    elif(value < 0):
        value = 0
    return value


# Not used
def request_color_in_rgb(red, green ,blue):
    hex_color = rgb_to_hex(red, green, blue)
    colors_json = request_colors(hex_color)

    if colors_json is not None:
        return colors_json['colors']

    return None

