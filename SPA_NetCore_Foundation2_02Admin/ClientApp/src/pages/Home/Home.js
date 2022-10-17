import React, { Component } from 'react';
import $ from 'jquery';
import { Link } from 'react-router-dom';


import parse from 'html-react-parser';
import { replace } from 'lodash';


import GlobalStatic from '@/Global/GlobalStatic.js';

export default class Admin extends Component
{


    constructor()
    {
        super();
    }

    render()
    {
        return (
            <div className="Test">
                Home
                <ul>
                    <Link to="/Admin/1"><li>1번상품</li></Link>
                    <Link to="/Admin/2"><li>2번상품</li></Link>
                </ul>
                
            </div>
        );
    }//end render
}

