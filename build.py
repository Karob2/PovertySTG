import os
import subprocess
import sys
import shutil
import json
import re
from distutils.dir_util import copy_tree

release_ref = [
    ["win", "win-x64", 0],
    ["win32", "win-x86", 0],
    ["linux", "linux-x64", 0],
    ["osx", "osx-x64", 0]
    ]

cmd_ref = [
    ["clean", 0],
    ["publish", 0],
    ["content", 0],
    ["pack", 0],
    ["icon", 0]
    ]

program_name = "The Heart's Illumination"

root = os.getcwd()

def main():
    if (len(sys.argv) < 2 or len(sys.argv) > 3):
        showUsage()

    release = sys.argv[1]
    found = 0
    for n in range(0, len(release_ref)):
        #print(cmd_ref[n][0])
        if (release == "all" or release == release_ref[n][0]):
            release_ref[n][2] = 1
            found = 1
    if (found == 0):
        showUsage()

    if len(sys.argv) == 3:
        cmd = sys.argv[2]
    else:
        cmd = "all"
    found = 0
    for n in range(0, len(cmd_ref)):
        #print(cmd_ref[n][0])
        if (cmd == "all" or cmd == cmd_ref[n][0]):
            cmd_ref[n][1] = 1
            found = 1
    if (found == 0):
        showUsage()

    #clean
    if (cmd_ref[0][1] == 1):
        print()
        print(">>> Cleaning source.")
        print()
        runProcess("dotnet clean Source/")

    for n in range(0, len(release_ref)):
        if release_ref[n][2] == 1:
            build_release(release_ref[n][1])

def build_release(release_name):
    publish_name = "publish-" + release_name

    #publish
    if (cmd_ref[1][1] == 1):
        print()
        print(">>> Building self-contained project (" + release_name + ").")
        print()
        removeDir(publish_name)
        #removeDir(os.path.join("Source", program_name, publish_name))
        runProcess("dotnet publish -r " + release_name + " -c release -o " + publish_name + " Source/")
        shutil.move(os.path.join("Source", publish_name), publish_name)

    #content
    if (cmd_ref[2][1] == 1):
        print()
        print(">>> Copying resources to Content folder (" + release_name + ").")
        print()
        checkDir("Source/ContentDL")
        checkDir(publish_name)
        #removeDir(os.path.join(publish_name, "Content"))
        copy_tree("Source/ContentDL", os.path.join(publish_name, "Content"))

    #pack
    if (cmd_ref[3][1] == 1):
        print()
        print(">>> Packing DLLs into lib directory (" + release_name + ").")
        print()
        changeDir(publish_name)
        with open(program_name + ".runtimeconfig.json", 'w') as file:
            file.write("{\"runtimeOptions\":{\"additionalProbingPaths\":[\"lib\"]}}")
        checkFile(program_name + ".deps.json")
        with open(program_name + ".deps.json") as f:
            data = json.load(f)
        for keyTarget in data["targets"]:
            dataTarget = data["targets"][keyTarget]
            for keyGroup in dataTarget:
                for keySubgroup in dataTarget[keyGroup]:
                    if keySubgroup != "runtime" and keySubgroup != "native": continue
                    for keyItem in dataTarget[keyGroup][keySubgroup]:
                        skip = False
                        filename = splitFilename(keyItem)
                        #print("......<" + filename[0] + "> " + filename[1] + " <" + filename[2] + ">")
                        if filename[2] != ".dll" and filename[2] != ".so" and filename[2] != ".dylib":
                            skip = True
                        if filename[1] == program_name or filename[1] == "hostfxr" or filename[1] == "libhostfxr" or filename[1] == "hostpolicy" or filename[1] == "libhostpolicy":
                            skip = True
                        if skip == True:
                            print("Skipping " + keyItem)
                            continue
                        targetDir = os.path.join("lib", keyGroup.lower(), filename[0])
                        targetFile = os.path.join("lib", keyGroup.lower(), keyItem)
                        sourceFile = filename[1] + filename[2]
                        makeDir(targetDir)
                        shutil.move(sourceFile, targetFile)
        changeDir(root)

    #icon
    if (cmd_ref[4][1] == 1):
        print()
        print(">>> Injecting icon into binary executable (" + release_name + ").")
        print()
        safe = True
        if sys.platform != "win32":
            print()
            print("Not implemented on your platform (" + sys.platform + ")! Skipping...")
            safe = False
        if release_name != "win-x64" and release_name != "win-x86":
            print()
            print("Cannot inject icon to " + release_name + " type binary. Skipping...")
            safe = False
        if safe == True:
            # since rcedit bungles file ownership, make a copy of the exe to restore default ownership properties
            forig = publish_name + "/" + program_name + ".exe"
            fdummy = publish_name + "/_" + program_name + ".exe"
            runProcess("Tools/rcedit-x64.exe \"" + forig + "\" --set-icon Source/Icon.ico")
            os.rename(forig, fdummy)
            shutil.copy2(fdummy, forig)
            os.remove(fdummy)

    print()
    print("Success!")
    #print("You may also want to remove *.pdb files if debugging the release builds is not necessary.")

def showUsage():
        print("Usage: python build.py [release] [command]")
        print("Releases:")
        print("    win, linux, osx, all")
        print("Commands:")
        print("    clean   Clean the project.")
        print("    publish Build release version.")
        print("    content Copy content to release folder.")
        print("    pack    Pack dlls into subdirectories.")
        print("    icon    Inject program icon.")
        print("    [blank] Run all commands.")
        exit()

def changeDir(str):
    try:
        os.chdir(str)
    except:
        print("Folder '" + str + "' not found.")
        print()
        print("Process failed with errors.")
        quit()

def checkDir(str):
    if (os.path.isdir(str) == False):
        print("Directory '" + str + "' does not exist.")
        print()
        print("Process failed with errors.")
        quit()

def makeDir(str):
    if (os.path.isdir(str) == False):
        os.makedirs(str)

def removeDir(str):
    if (os.path.isdir(str)):
        shutil.rmtree(str)

def checkFile(str):
    if (os.path.isfile(str) == False):
        print("File '" + str + "' does not exist.")
        print()
        print("Process failed with errors.")
        quit()

def splitFilename(str):
    nExt = 0
    nPath = 0
    splitname = [0, 0, 0]
    for i in range(0, len(str)):
        if str[i] == ".":
            nExt = i
        if str[i] == "/" or str[i] == "\\":
            nPath = i + 1
    if nExt <= nPath:
        splitname[0] = str[:nPath]
        splitname[1] = str[nPath:]
        splitname[2] = ""
        return splitname

    splitname[0] = str[:nPath]
    splitname[1] = str[nPath:nExt]
    splitname[2] = str[nExt:]
    return splitname

def runProcess(str):
    print("Execute '" + str + "'")
    result = subprocess.run(str, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    print()
    print("stdout=")
    print(result.stdout)
    #print(result.stdout.decode("unicode_escape"))
    if (result.returncode != 0):
        print("Command failed!")
        print()
        print("stderr=")
        print(result.stderr)
        #print(result.stderr.decode("unicode_escape"))
        print()
        print("Process failed with errors.")
        quit()
    return result.returncode

if __name__ == '__main__':
    main()
