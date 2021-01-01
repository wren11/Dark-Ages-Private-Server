



#frames 0-4 - move up,  5-9 - move down,   10-11 - attack up 12-13 - attack down
import os

movement_frame_offs = 0 # frame number of movement up
movement_frame_count = 5 # count of frames of movement in one directions
attack_frame_offs = 10
attack_frame_count = 2

stopFrameIndex = 0 # I dont know the exact meaning of these frame offsets but usually they are same as walk_indices
stopFrameCount = 2 # I dont know the exact meaning of these frame offsets but usually they are same as walk_indices
stopMotionFrameCount = 2
stopMotionProbability = 2


transparent_color = [0,0,0] # white
transp_cmd = ":".join(str(v) for v in transparent_color)



#because we have only one type of attack we will repeate attack frame block 3 times
indices = str(movement_frame_offs) + ":" + str(movement_frame_count) + ":" \
    + str(attack_frame_offs) + ":"  + str(attack_frame_count) + ":"  \
    + str(attack_frame_offs) + ":"  + str(attack_frame_count) + ":"  \
    + str(attack_frame_offs) + ":"  + str(attack_frame_count) + ":"  \
    + str(stopFrameIndex) + ":"  + str(stopFrameCount) + ":"  \
    + str(stopMotionFrameCount) + ":"  + str(stopMotionProbability)\



	
for i in range(10):
    cmd_line = "bmp2epf.exe --frame *.png --frame_indices %s --pal_num 129 --mpf_coord %s:%s:0:0 MNS%s.MPF" % (indices,i*10,i*10,169 + i)
    print(cmd_line)
    os.system(cmd_line)
	
	
os.system("pause")


