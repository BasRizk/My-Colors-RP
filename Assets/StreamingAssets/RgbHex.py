# Module to hold RGB & Hex Conversions
    
def rgb_to_hex(red, green, blue):
    return dec_to_hex(red) + dec_to_hex(green) + dec_to_hex(blue)

def dec_to_hex(decimal):
    if decimal < 16:
        return '0' + get_hex_digit(decimal)
    return dec_to_hex_helper(decimal)
    
def dec_to_hex_helper(decimal):
    if decimal < 16:
        return get_hex_digit(decimal)
    
    hex_div = decimal // 16
    hex_reminder = decimal % 16
    
    if hex_reminder >= 0:
        return dec_to_hex_helper(hex_div) + get_hex_digit(hex_reminder)

def get_hex_digit(hex_decimal):
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
