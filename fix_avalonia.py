import sys

with open(".github/workflows/coverage-orchestrator.yml", "r") as f:
    lines = f.readlines()

avalonia_start = -1
for i, line in enumerate(lines):
    if "  avalonia:" in line:
        avalonia_start = i
        break

if avalonia_start != -1:
    install_step_start = -1
    for i in range(avalonia_start, len(lines)):
        if "name: Install coverlet.console" in lines[i]:
            install_step_start = i - 1
            break
    
    validate_step_start = -1
    for i in range(avalonia_start, len(lines)):
        if "uses: ./.github/actions/validate-cobertura" in lines[i]:
            validate_step_start = i - 1
            break
            
    if install_step_start != -1 and validate_step_start != -1:
        new_avalonia_steps = [
            "      - name: Setup coverlet.runsettings\n",
            "        shell: bash\n",
            "        working-directory: target\n",
            "        run: |\n",
            "          cat <<INNEREOF > coverlet.runsettings\n",
            "          <?xml version=\"1.0\" encoding=\"utf-8\"?>\n",
            "          <RunSettings>\n",
            "            <DataCollectionRunSettings>\n",
            "              <DataCollectors>\n",
            "                <DataCollector friendlyName=\"XPlat Code Coverage\">\n",
            "                  <Configuration>\n",
            "                    <Format>cobertura</Format>\n",
            "                    <Include>[Avalonia*]*</Include>\n",
            "                  </Configuration>\n",
            "                </DataCollector>\n",
            "              </DataCollectors>\n",
            "            </DataCollectionRunSettings>\n",
            "          </RunSettings>\n",
            "          INNEREOF\n",
            "\n",
            "      - name: Run tests with XPlat Code Coverage\n",
            "        shell: bash\n",
            "        working-directory: target\n",
            "        run: |\n",
            "          mkdir -p coverage-results\n",
            "          UNIT_TESTS=( \"src/Avalonia.Base.UnitTests/Avalonia.Base.UnitTests.csproj\" \"src/Avalonia.Controls.UnitTests/Avalonia.Controls.UnitTests.csproj\" \"src/Avalonia.Input.UnitTests/Avalonia.Input.UnitTests.csproj\" \"src/Avalonia.Interactivity.UnitTests/Avalonia.Interactivity.UnitTests.csproj\" \"src/Avalonia.Layout.UnitTests/Avalonia.Layout.UnitTests.csproj\" \"src/Avalonia.Markup.UnitTests/Avalonia.Markup.UnitTests.csproj\" \"src/Avalonia.Markup.Xaml.UnitTests/Avalonia.Markup.Xaml.UnitTests.csproj\" \"src/Avalonia.Visuals.UnitTests/Avalonia.Visuals.UnitTests.csproj\" \"src/Avalonia.Skia.UnitTests/Avalonia.Skia.UnitTests.csproj\" )\n",
            "          for proj in \"${UNIT_TESTS[@]}\"; do\n",
            "            dotnet test \"$proj\" --collect:\"XPlat Code Coverage\" --settings coverlet.runsettings --results-directory ./coverage-results --no-build || echo \"::warning::Tests in $proj failed\"\n",
            "          done\n",
            "          mkdir -p TestResults\n",
            "          dotnet-coverage merge -r -f cobertura -o TestResults/coverage.cobertura.xml \"coverage-results/**/*.cobertura.xml\"\n"
        ]
        lines[install_step_start:validate_step_start] = new_avalonia_steps

with open(".github/workflows/coverage-orchestrator.yml", "w") as f:
    f.writelines(lines)
