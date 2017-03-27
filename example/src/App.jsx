import React from 'react';
import TOUIValues from './TOUIValues';

import Appbar from 'muicss/lib/react/appbar';
import Button from 'muicss/lib/react/button';
import Container from 'muicss/lib/react/container';

const MainBar = () => {
  const styles = {
    verticalAlign: 'middle',
    textAlign: 'center'
  };

  return <Appbar>
            <table width="100%">
              <tbody>
                <tr style={styles}>
                  <td className="mui--appbar-height">
                    <h1 className="mui--text-display2">TOUI Example</h1>
                  </td>
                </tr>
              </tbody>
            </table>
         </Appbar>
}

const App = () => (
  <div>
    <MainBar/>
    <Container>
      <TOUIValues />
    </Container>
  </div>
)

export default App
