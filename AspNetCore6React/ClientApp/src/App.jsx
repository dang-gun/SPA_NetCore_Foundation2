import React, { Component } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

import './App.css';

//관리자 페이지
import Admin from '@/pages/Admin/Admin.js';
//일반 페이지
import Home from '@/pages/Home/Home.js';


function App()
{
    return (
        <div className="App">
            <BrowserRouter>
                <header className="App-header">
                    공통 헤더
                </header>

                <Routes>
                    <Route path="/" element={<Home />}></Route>
                    <Route path="/Admin/*" element={<Admin />}></Route>
                </Routes>

            </BrowserRouter>
        </div>

        
    );
}

export default App;
