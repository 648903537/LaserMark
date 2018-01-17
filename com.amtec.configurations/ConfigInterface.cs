using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.configurations
{
    interface ConfigInterface
    {
        void passConfig(ApplicationConfiguration config, IMSApiSessionContextStruct sessionContext);
    }
}
