#!/bin/bash

echo "zbar-sharp documentation generation script"
echo "=========================================="
echo ""
echo "This will clear the docs/ folder,"
read -p "Hit ctrl+c to abort, and [Enter] to continue:"
echo ""

#Check if 
if [ -d "docs" ]; then
	echo "Deleting files from docs/";
	rm docs/*;
else
	echo "Cloning gh-pages into a docs/";
	git clone --reference . -b gh-pages -- `git config --get "remote.origin.url"` docs;
	cd docs/;
	git rm --ignore-unmatch * > /dev/null;
	cd ..;
fi;

#Run doxygen
echo "";
if [[ "$#" == "1" && "$1" == "--verbose" ]]; then
	echo "Running doxgen";
	doxygen Doxyfile;
else
	echo "Running doxgen, use --verbose for warnings";
	doxygen Doxyfile > /dev/null 2> /dev/null;
fi;

echo "Documentation generation completed."
echo " - Checkout and publish the documentation"
