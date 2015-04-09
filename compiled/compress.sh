echo "removing old versions"
rm pagi_*_newest.zip
echo "compressing linux version"
d=$(date +'%m.%d.%Y')
zip -r pagi_linux_newest.zip pagi_linux.x86 pagi_linux.x86_64 pagi_linux_Data 
cp pagi_linux_newest.zip "pagi_linux_${d}.zip"

echo "compressing mac version"
zip -r pagi_mac_newest.zip pagi_mac.*
cp pagi_mac_newest.zip "pagi_mac_${d}.zip"

echo "compressing windows version"
zip -r pagi_windows_newest.zip pagi_windows.exe pagi_windows_Data
cp pagi_windows_newest.zip "pagi_windows_${d}.zip"
