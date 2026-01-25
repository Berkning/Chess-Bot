
# OpenBench variables
EXE ?= LichessBot

# Paths
PROJECT := ../LichessBot.csproj
OUTDIR  := .

# Build configuration
CONFIG  := Release
RUNTIME := linux-x64

.PHONY: all clean

all:
	dotnet publish $(PROJECT) \
		-c $(CONFIG) \
		-r $(RUNTIME) \
		--self-contained true \
		/p:PublishSingleFile=true \
		/p:IncludeNativeLibrariesForSelfExtract=true \
		-o $(OUTDIR)

	mv $(OUTDIR)/LichessBot $(OUTDIR)/$(EXE)

clean:
	rm -f $(EXE)
	rm -rf bin obj