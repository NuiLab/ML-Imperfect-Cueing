import json
import os
from glob import glob
import shutil


files = []

data = {}
f = None
name = ''
file = ''

for json_file in glob('Dataset*/captures*'):
    f = open(json_file)
    data = json.load(f)

    for i in data['captures']:
        file = ''
        for t in i['annotations']:
            for x in t['values']:
                w = float(x['width']) / 576.0
                h = float(x['height']) / 324.0
                x_c = (float(x['x']) / 576.0) + (w / 2.0)
                y_c = (float(x['y']) / 324.0) + (h / 2.0)
                file += str(x['label_id'])+' '+str(x_c)+' '+str(y_c)+' '+str(w)+' '+str(h)+'\n' # class x_center y_center width height
        name = 'labels/'+str(i['filename'].split('/')[1][:-4])+'.txt'
        with open(name, 'a') as filef:
            filef.write(file)

    f.close()
