# -*- coding: utf-8 -*-
"""
# Module to hold RGB & Hex Conversions
"""
 
def rgb_to_hex(red, green, blue):
    """
    Returns a 6 digit hex string converted fom rgb values.
    """
    return dec_to_hex(red) + dec_to_hex(green) + dec_to_hex(blue)

def dec_to_hex(decimal):
    """
    Returns a hex (at least 2 digits) value converted from decimal value.
    """
    if decimal < 16:
        return '0' + _get_hex_digit(decimal)
    return _dec_to_hex_helper(decimal)
    
def _dec_to_hex_helper(decimal):
    if decimal < 16:
        return _get_hex_digit(decimal)
    
    hex_div = decimal // 16
    hex_reminder = decimal % 16
    
    if hex_reminder >= 0:
        return _dec_to_hex_helper(hex_div) + _get_hex_digit(hex_reminder)

def _get_hex_digit(hex_decimal):
    """
    Returns a hex digit converted from a decimal number between 1 and 15.
    """
    hex_digits = {
            '15' : 'F',
            '14' : 'E',
            '13' : 'D',
            '12' : 'C',
            '11' : 'B',
            '10' : 'A',
            }
    if(hex_decimal > 9):
        return hex_digits[str(hex_decimal)]
    else:
        return str(hex_decimal)
