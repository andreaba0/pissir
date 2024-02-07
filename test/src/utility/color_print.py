class ColorPrint:
    colors = {
        'HEADER': '\033[95m',
        'GRAY': '\033[90m',
        'OKBLUE': '\033[94m',
        'OKGREEN': '\033[92m',
        'WARNING': '\033[93m',
        'FAIL': '\033[91m',
        'ENDC': '\033[0m',
        'BOLD': '\033[1m',
        'UNDERLINE': '\033[4m'
    }
    
    def print(spaces, *msg_array):
        colors = ColorPrint.colors
        for line in msg_array:
            strout = ""
            strout += ' ' * spaces
            for msg in line:
                for i in range(0, len(msg)-1):
                    opt = msg[i]
                    strout += f"{colors[opt]}"
                strout += f"{msg[-1]}"
                strout += f"{colors['ENDC']}"
            print(strout)