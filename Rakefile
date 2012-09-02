require "rubygems"
require "bundler"
Bundler.setup
$: << './'

require 'albacore'
require 'semver'
require 'aws/s3'
require 'rake/clean'

require 'tools/buildscripts/environment'
require 'tools/buildscripts/utils'

CLOBBER.include("build/*")

task :default => [ :clobber, "env:release", :version, :build, :test, :output, :zip_webui, :zip, :msi ]

desc "Build"
msbuild :build => :version do |msb|
    msb.properties :configuration => "Release"
    msb.targets :Clean, :Build
    msb.solution = "Hadouken.sln"
end

desc "Versioning"
assemblyinfo :version => "env:common" do |asm|
    asm.version = BUILD_VERSION
    asm.file_version = BUILD_VERSION
    
    asm.company_name = "Hadouken"
    asm.product_name = "Hadouken"
    asm.copyright = "2012"
    asm.namespaces = "System", "System.Reflection", "System.Runtime.InteropServices", "System.Security"
    
    asm.custom_attributes :AssemblyInformationalVersion => "#{BUILD_VERSION}", # disposed as product version in explorer
        :CLSCompliantAttribute => false,
        :AssemblyConfiguration => "#{CONFIGURATION}"
    
    asm.com_visible = false
    
    asm.output_file = "src/Shared/CommonAssemblyInfo.cs"
end

desc "Test"
nunit :test => :build do |nunit|
    FileUtils.mkdir_p "build/reports" unless FileTest.exists?("build/reports")
    
    if(ENV['TEAMCITY_VERSION'])
        nunit.command = "#{ENV['teamcity.dotnet.nunitlauncher2.0']}"
        nunit.options = "v4.0 x86 NUnit-2.6.0"
    else
        nunit.command = "tools/nunit-2.6.0.12051/bin/nunit-console-x86.exe"
        nunit.options "/framework:v4.0.30319 /xml:build/reports/nunit.xml"
    end
    
    nunit.assemblies "src/Tests/Hadouken.UnitTests/bin/#{CONFIGURATION}/Hadouken.UnitTests.dll"
end

desc "Output"
task :output => :build do
    copy_files "src/Hosts/Hadouken.Hosts.CommandLine/bin/Release/", "*.{dll,exe}", "build/hdkn-#{BUILD_VERSION}"
    copy_files "src/Hosts/Hadouken.Hosts.WindowsService/bin/Release/", "*.{dll,exe}", "build/hdkn-#{BUILD_VERSION}"
    copy_files "src/Config/Release/", "*.{config}", "build/hdkn-#{BUILD_VERSION}"    
end

desc "Zip"
zip :zip => :output do |zip|
    zip.directories_to_zip "build/hdkn-#{BUILD_VERSION}"
    zip.output_file = "hdkn-#{BUILD_VERSION}.zip"
    zip.output_path = "#{File.dirname(__FILE__)}/build/"
end

desc "Zip WebUI"
zip :zip_webui do |zip|
    zip.directories_to_zip "src/WebUI"
    zip.output_file = "webui.zip"
    zip.output_path = "#{File.dirname(__FILE__)}/build/hdkn-#{BUILD_VERSION}/"
end

desc "MSI"
task :msi => :output do
    system "tools/wix-3.6rc/candle.exe -ext WixUtilExtension -dBuildVersion=#{BUILD_VERSION} -dBinDir=build/hdkn-#{BUILD_VERSION} -out src/Installer/ src/Installer/Hadouken.wxs src/Installer/SettingsDialog.wxs"
    system "tools/wix-3.6rc/light.exe -ext WixUIExtension -ext WixUtilExtension -sval -pdbout src/Installer/Hadouken.wixpdb -out build/hdkn-#{BUILD_VERSION}.msi src/Installer/Hadouken.wixobj src/Installer/SettingsDialog.wixobj"
end

desc "Publish to Amazon S3"
task :publish do
    AWS::S3::Base.establish_connection!(
        :access_key_id     => ENV['S3_ACCESS_KEY'],
        :secret_access_key => ENV['S3_SECRET_KEY']
    )
    
    Dir.glob('build/*.{msi,zip}') { |file|
        AWS::S3::S3Object.store(File.basename(file), 
            open(file), 
            ENV['S3_BUCKET'])
    }
end