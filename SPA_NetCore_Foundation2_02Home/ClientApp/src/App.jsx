import React, { Component } from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';

import './App.css';


//일반 페이지
import Home from '@/pages/Home/Home.js';
//마이 페이지
import MyPage from '@/pages/MyPage/MyPage.js';


function App()
{
    return (
        <div className="App">
            <BrowserRouter >
                <header className="App-header">
                    홈 페이지
                    <ul>
                        <Link to="/"><li>home</li></Link>
                        <Link to="/MyPage/"><li>MyPage</li></Link>
                    </ul>
                </header>

                <Routes>
                    <Route path="/" element={<Home />}></Route>
                    <Route path="/MyPage/" element={<MyPage />}></Route>
                </Routes>

            </BrowserRouter>
        </div>

        
    );
}

export default App;
