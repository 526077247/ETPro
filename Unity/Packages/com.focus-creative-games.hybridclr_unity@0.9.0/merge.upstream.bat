@echo off

git checkout .
git fetch upstream
git merge upstream/main
git push

sed -n '/version/p' package.json > tmp.txt
sed -i 's/[version,",: ]*//g' tmp.txt
for /f "delims=" %%i in (tmp.txt) do set var=%%i
echo ×îÐÂ°æ±¾ºÅ:%var%
del tmp.txt

git tag %var%
git push origin --tags

pause
