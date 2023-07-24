cmake_minimum_required(VERSION 2.8.12)

project(csm)

add_library(csm MODULE
    lib.cpp)

install(TARGETS csm DESTINATION .)
