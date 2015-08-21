# Semver-AssemblyCompatibility
Helps treat assembly compatibility as a semantically versioned interface. This is useful for long nuget dependency chains caused by poor architecture decisions. An idea of the rationale behind this: https://grahamthecoder.wordpress.com/2014/12/10/whats-in-a-strong-name/

I have a powershell script which does a post-build check for this system on [Codeplex](https://hg.codeplex.com/forks/grahamhelliwell/diffonly). I'm essentially porting that system to Roslyn then extending it.


#### Milestones
* [ ] Use Roslyn to write out an assembly's public API
* [ ] Generate a semantic version based on the latest public API, it's verison and an unversioned public API
* [ ] Generate a semantic version from a nuspec (by finding the included projects and versioning their APIs together)
* [ ] Create a unit test to run locally which writes out the version to AssemblyInfo and Nuspec
* [ ] Write guidance on setting up TeamCity to replace just build counter
* [ ] Include powershell script to replace build counter on an arbitrary build system
* [ ] Store something on GitHub, TC or Nuget to allow build counters to work on a per branch basis

#### Future ideas
Allow changes to be planned and enforced so that a number of breaking changes can be easily batched up.
```
[UpcomingApi("9.0", ChangeType.Accessibility, Accessibility.Internal, Description = "Please use x instead", Level = WarnLevel.Warning)]
[UpcomingApi("10.0", ChangeType.RenamedTo, "MyClass2")]
[PreviousApi("10.0", ChangeType.RenamedFrom, "MyClass1")]
```
These would appear as information items by default when used by consuming projects. For the source project, when the API is at or above the relevant version, a warning would be emitted (and there'd be a test method that could be used on build server before publishing).
Could add "code fixes" in source and consuming solution for dealing with this change (a bit like Type Forwards but at a source level)
