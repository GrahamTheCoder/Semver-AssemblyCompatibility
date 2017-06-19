# Semver-AssemblyCompatibility
Helps treat assembly compatibility as a semantically versioned interface. This is useful for long nuget dependency chains caused by poor architecture decisions. An idea of the rationale behind this: https://grahamthecoder.wordpress.com/2014/12/10/whats-in-a-strong-name/

The intention is to have something which masquerades as a unit test in your project which is always validating the API against a known version. Optionally it could also update the version number when it's incorrect.

Since SemVer only uses 3 parts of the version number, the build number can be set separately. I recommend just increment as far as possible then reset.

## Details
I have an existing powershell script which does a post-build check for this system on [Codeplex](https://hg.codeplex.com/forks/grahamhelliwell/diffonly). I'm essentially porting that system to Roslyn then extending it.

Many changes *could* end up being a breaking change under various circumstances in a consuming project. This project aims to hit:
* All cases that are guaranteed to break a consumer (e.g. removing a type that's used)
* Zero of the cases where the consumer was using reflection to break accessibility rules

#### Milestones
* [x] Use Roslyn to write out an assembly's public API
  * [x] Types
  * [x] Methods and properties
  * [x] Events
  * [x] Delegates
  * [x] Base type and interface info
* [x] Generate a semantic version based on (the latest public API, its verison, and an unversioned public API)
* [ ] Generate a semantic version from a nuspec (by finding the included projects and versioning their APIs together)
* [ ] Create a public method that writes out a JSON file containing version and API
* [ ] Include example powershell script and unit test which call the above method and make use of the version to update assembly infos and nuspecs

#### Future ideas
##### Provide other version inputs and/or make inputs pluggable
So that other things can be considered breaking changes. For example, scan the recorded output files for characterization tests for changes and allow those to cause minor/major version increase.

##### Allow changes to be planned and enforced
So that a number of breaking changes can be easily batched up.
```
[UpcomingApi("9.0", ChangeType.Accessibility, Accessibility.Internal, Description = "Please use x instead", Level = WarnLevel.Warning)]
[UpcomingApi("10.0", ChangeType.RenamedTo, "MyClass2")]
[PreviousApi("10.0", ChangeType.RenamedFrom, "MyClass1")]
```
These would appear as information items by default when used by consuming projects. For the source project, when the API is at or above the relevant version, a warning would be emitted (and there'd be a test method that could be used on build server before publishing).
Could add "code fixes" in source and consuming solution for dealing with this change (a bit like Type Forwards but at a source level)


