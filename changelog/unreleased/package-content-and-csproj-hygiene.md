type: fix

The `-Core-EF6`..`-Core-EF10` and `-EF6` packages no longer ship the icon images under `content/` and `contentFiles/` (they were copied into consuming projects), no package asks for license acceptance on install (all are MIT), and the libraries no longer reference `coverlet.collector` or embed a Win32 icon.
