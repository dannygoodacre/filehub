#!/bin/bash

dotnet test \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=coverage \
    /p:ExcludeByFile="**/Data/**/*.cs%2c**/Models/**/*.cs%2c**/Middleware/**/*.cs%2c**/Program.cs" \
 
reportgenerator \
    -reports:"coverage.cobertura.xml" \
    -targetdir:"coveragereport" \
    -reporttypes:Html
