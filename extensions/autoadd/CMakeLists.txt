cmake_minimum_required(VERSION 2.8 FATAL_ERROR)

project("autoadd")

add_library(
    autoadd
    SHARED
    manifest.cpp
    autoaddextension.cpp
)

target_link_libraries(
    autoadd
    hadouken
)
