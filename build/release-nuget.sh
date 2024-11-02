#!/bin/bash

# Usage: ./release.sh <command>
# Commands: prepare, publish

WORKING_DIR="src/xpd"
CS_PROJ_FILE="$WORKING_DIR/xpd.csproj"
NUGET_API_KEY="your-nuget-api-key"
NUGET_SOURCE="https://api.nuget.org/v3/index.json"

get_version() {
    xmllint --xpath "string(//Version)" "$CS_PROJ_FILE"
}

get_package_name() {
    xmllint --xpath "string(//PackageId)" "$CS_PROJ_FILE"
}

print_error() {
  local message="$1"
  echo -e "\033[31m$message\033[0m"
}

underline() {
  local message="$1"
  echo "$(tput smul)$message$(tput rmul)"
}

prepare() {
    echo "Incrementing version and generating changelog with versionize..."

    if ! dotnet versionize --workingDir "$WORKING_DIR" --skip-commit --skip-tag --exit-insignificant-commits; then
        echo -n "There are no feat or fix commits since last release. Changelog will be empty. Continue anyway? [y/$(underline "n")] "
        read -r response
        if [[ "${response:l}" =~ ^(yes|y)$ ]]; then
            echo "Continuing..."
            dotnet versionize --workingDir "$WORKING_DIR" --skip-commit --skip-tag
        else
            echo "Exiting without increasing version."
            exit 1
        fi
    fi
}

publish() {
    VERSION=$(get_version)

    if git diff --exit-code && git diff --cached --exit-code; then
        echo -n "
There are no changes to commit.
Usually you should run prepare command before publish to increment version number and generate changelog.
Are you sure you want to publish?
Version tag will be added to the last commit. [y/$(underline "n")] "

        read -r response
        if [[ -z "$response" || "${response:l}" =~ ^(no|n)$ ]]; then
            echo "Exiting without publishing."
            exit 1
        fi
    else
        echo "Committing changes..."
        git add .
        git commit -m "chore(release): $VERSION" --no-verify
    fi

    echo "Tagging commit with version 'v$VERSION'..."
    if git tag -l "v$VERSION" > /dev/null; then
        print_error "Tag already exists. Exiting."
        exit 1
    else
        git tag "v$VERSION"
    fi

    echo "Packing the project in Release mode..."
    dotnet pack "$WORKING_DIR" --configuration Release --verbosity quiet

    echo "Publishing to NuGet..."
    dotnet nuget push "$WORKING_DIR"/nupkg/*.nupkg --api-key "$NUGET_API_KEY" --source "$NUGET_SOURCE"

    PACKAGE_NAME=$(get_package_name)

    echo "Published NuGet package: https://www.nuget.org/packages/$PACKAGE_NAME/$VERSION"
}

case "$1" in
    prepare)
        prepare
        ;;
    publish)
        publish
        ;;
    *)
        echo "Usage: $0 {prepare|publish}"
        exit 1
        ;;
esac