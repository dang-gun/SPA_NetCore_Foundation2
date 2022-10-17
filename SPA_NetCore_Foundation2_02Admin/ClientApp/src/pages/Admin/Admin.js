import React, { Component } from 'react';
import $ from 'jquery';

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
                관리 페이지
            </div>
        );
    }//end render
}

