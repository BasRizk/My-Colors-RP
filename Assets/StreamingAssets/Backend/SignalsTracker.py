# -*- coding: utf-8 -*-

from DataManager import get_brain_signals_path
from BrainAnalysis import analyze_past_learning_data
from CyKIT import eeg
import time
import threading
import socket

def run_CyKIT():  
    import sys
    sys.argv = ["","","","",""]
    sys.argv[1] = "127.0.0.1"
    sys.argv[2] = "55555"
    sys.argv[3] = "5"
    sys.argv[4] = "info+format-0+"
    
    import CyKIT.CyKITv2

def catch_data():
    # create an INET, STREAMing socket
    conn = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # now connect to the web server on port 80 - the normal http port
    conn.connect(("localhost", 18000))
    
    while True:
        data = conn.recv(1024)
        if not data:
            break
        conn.send(data)
    conn.close()

def track_signals(aquisation_period):
    print('Signal Tracking to begin.')
    #device_sn = "UD201605030023C6"
    
    max_time = aquisation_period
    start_time = time.time()  # remember when we started
    
    output_file_path = get_brain_signals_path()
    
    if 'CyINIT' not in locals():
        #global CyINIT
        CyINIT = 2
    
    myIO = eeg.MyIO()
    model_num = 5 # EPOC+ (Research)
    parameters = [] # TODO
    headset = eeg.EEG(model_num, myIO, parameters).start()

    
    while CyINIT >= 2 and (time.time() - start_time) < max_time:
        CyINIT += 1
        
        if CyINIT > 1000:
            modelCheck = myIO.modelChange()
            if modelCheck != 0:
                MODEL = modelCheck
            
            CyINIT = 2
            check_threads = 0
            #print "testing"
            
            for t in threading.enumerate():
                if t.getName() == "eegThread":
                    check_threads += 1
            
            if check_threads == 1:
                #ioTHREAD.onClose()
                print("*** Reseting . . .")
                CyINIT = 1
                #main(1)
    if CyINIT < 2:
        print('Error: Threads died')

    headset.stopRecord()

    print('SIGNAL_TRACKING_COMPLETED')
    print('Signal Analysing to begin.')
    analyze_past_learning_data()
    print('SIGNAL_ANALYSIS_COMPLETED')
    