// Every test spins up its own in-process API with its own SQLite file, so the suite is run
// sequentially to keep the timing-sensitive scenarios (parallel requests, channel bursts) stable.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
