cd C:\da\bmp2epf\dll_exe\wasp_demo
python b2e.py wasp20.json
pause
copy MNS001.MPF C:\da\Pack\hades\MNS001.MPF
cd C:\da\Pack
C:\da\Pack\pack_hades.bat
copy C:\da\Pack\hades.dat "C:\Program Files (x86)\KRU\Dark Ages\hades.dat" /Y
pause