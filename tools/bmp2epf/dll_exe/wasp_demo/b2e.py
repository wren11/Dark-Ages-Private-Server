import inspect
import json
import sys
import os
from collections import OrderedDict


class JsonObj(object):
    def __init__(self):
        pass


    def json_val(self,var):
        if isinstance(var, JsonObj):
            return var.to_json()
        elif isinstance(var, list):
            return [self.json_val(v) for v in var]
        else:
            return var

    def to_json(self):
        vars = inspect.getmembers(self, predicate= lambda x: not inspect.ismethod(x))
        vars = [var for var in vars if not var[0].startswith("_")]
        d = OrderedDict({name: self.json_val(var) for name,var in vars})
        return d

    def from_json(self, dct):
        vars = inspect.getmembers(self, predicate= lambda x: not inspect.ismethod(x))
        vars = [var for var in vars if not var[0].startswith("_")]
        for name, val in vars:
            setattr(self, name, dct[name])


class FrameData(JsonObj):
    def __init__(self,fname = ""):
        super(FrameData,self).__init__()
        self.filename = fname
        self.left = 0
        self.top = 0
        self.right = 0
        self.bottom = 0
        self.x_offs = 0
        self.y_offs = 0


class MpfProject(JsonObj):
    def __init__(self):
        super(MpfProject,self).__init__()
        self.ouput_name = ""
        self.zinput_files = list()
        self.walkFrameIndex = 0
        self.walkFrameCount = 0
        self.stopFrameIndex = 0
        self.stopFrameCount = 0
        self.stopMotionFrameCount = 0
        self.stopMotionProbability = 0
        self.attackFrameIndex = 0
        self.attackFrameCount = 0
        self.attack2FrameIndex = 0
        self.attack2FrameCount = 0
        self.attack3FrameIndex = 0
        self.attack3FrameCount = 0
        self.pallete_number = 0
        self.pallete_file = ""
        self.transparent = []
        self.width = 0
        self.height = 0


    def set_filenames(self,fnames):
        self.zinput_files = [FrameData(fname) for fname in fnames]

    def is_mpf(self):
        return self.ouput_name.split(".")[-1].upper().endswith("MPF")

    def command_line(self):
        cmd = ["bmp2epf",]

        if self.height:
            cmd.append("--height %s" % self.height)

        if self.width:
            cmd.append("--width %s" % self.width)


        frames = " ".join(["--frame %s  --frame_coord %s:%s:%s:%s:%s:%s "
                          % (fd.filename, fd.left, fd.top, fd.right, fd.bottom, fd.x_offs, fd.y_offs) for fd in self.zinput_files])
        cmd.append(frames)

        if self.is_mpf():
            indices = "--frame_indices %s:%s:%s:%s:%s:%s:%s:%s:%s:%s:%s:%s" % (self.walkFrameIndex,self.walkFrameCount,
                                                               self.attackFrameIndex,self.attackFrameCount,
                                                               self.attack2FrameIndex, self.attack2FrameCount,
                                                               self.attack3FrameIndex, self.attack3FrameCount,
                                                               self.stopFrameIndex,self.stopFrameCount,
                                                               self.stopMotionFrameCount, self.stopMotionProbability
                                                               )
            if self.attack2FrameIndex == -1 or  self.attack2FrameCount == -1 or \
                self.attack3FrameIndex == -1 or  self.attack3FrameCount == -1:
                indices = "--frame_indices %s:%s:%s:%s:%s:%s:%s:%s" % (self.walkFrameIndex, self.walkFrameCount,
                                                                       self.attackFrameIndex,
                                                                       self.attackFrameCount,
                                                                       self.stopFrameIndex,
                                                                       self.stopFrameCount,
                                                                       self.stopMotionFrameCount,
                                                                       self.stopMotionProbability
                                                                       )
            cmd.append(indices)

        if self.pallete_file:
            cmd.append("--pallete %s" % self.pallete_file)
        if self.is_mpf() and self.pallete_number:
            cmd.append("--pal_num %s" % self.pallete_number)
        if self.transparent:
            cmd.append("--transparent " + ":".join(str(v) for v in self.transparent))

        cmd.append(self.ouput_name)

        return " ".join(cmd)


    def from_json(self, dct):
        vars = inspect.getmembers(self, predicate= lambda x: not inspect.ismethod(x))
        vars = [var for var in vars if not var[0].startswith("_")]
        for name, val in vars:
            if name == "zinput_files":
                del self.zinput_files[:]
                for val in dct[name]:
                    a = FrameData()
                    a.from_json(val)
                    self.zinput_files.append(a)
            else:
                setattr(self, name, dct[name])




def usage():
    print("Usage:")


def create_json_template(fname):
    proj = MpfProject()
    f = [v for v in os.listdir(".") if v.split(".")[-1].upper() in ["BMP","PNG"]]
    f.sort()
    proj.set_filenames(f)
    f = open(fname,"w")
    dct = proj.to_json()
    json.dump(dct,f,indent =4)
    f.close()


def execute_json_file(fname):
    fp = open(fname)
    os.chdir(os.path.dirname(os.path.abspath(fname)))
    print(os.getcwd())
    prj = MpfProject()
    dct = json.load(fp)
    prj.from_json(dct)

    line = prj.command_line()
    print(line)
    os.system(line)
    os.system("pause")



def main():
    if len(sys.argv) == 1:
        create_json_template("out.json")
    elif len(sys.argv) == 2:
        execute_json_file(sys.argv[1])


if __name__=="__main__":
    main()